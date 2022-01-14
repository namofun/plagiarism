using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Jobs;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Worker
{
    public class Report
    {
        private readonly ICosmosConnection _connection;
        private readonly ReportGenerator _reporter;

        public Report(
            ICosmosConnection connection,
            IConvertService2 converter,
            ICompileService compiler,
            IReportService reporter)
        {
            _connection = connection;
            _reporter = new(compiler, converter, reporter);
        }

        [FunctionName("Report")]
        public async Task Run(
            [QueueTrigger(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] string reportRequest,
            [Queue(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportContinuation,
            ILogger log)
        {
            string queueStamp = reportRequest.Split('%')[0];

            _reporter.SetLogger(log);
            IJobContext store = new CosmosStoreService(_connection);
            var lru = new LruStore<(string, int), (Submission, Frontend.Submission)>();
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMinutes(9));

            var w = await _connection.Reports.GetContainer().Scripts.ExecuteStoredProcedureAsync<string>("testanddequeue", new("xx"), new[] { "x" });

            log.LogInformation("Report worker started on {StartTime} for '{QueueMessage}'.", DateTimeOffset.Now, reportRequest);

            bool shouldContinue = true;
            while (!cts.Token.IsCancellationRequested)
            {
                shouldContinue = await _reporter.DoWorkBatchAsync(store, lru);
                if (!shouldContinue) break;
            }

            lru.Clear();
            if (shouldContinue)
            {
                // There is an active work item last iteration, but worker stopped here.
                // This happens when job is running out of time.
                Guid continuationId = Guid.NewGuid();
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string continuationRecord = $"continuation|{timestamp}|{continuationId}%{queueStamp}";
                await reportContinuation.AddAsync(continuationRecord);
                await reportContinuation.FlushAsync();
                log.LogInformation("Report continuation emitted with '{ContinuationId}'.", continuationRecord);
            }
            else
            {
                log.LogInformation("Report worker didn't pick up any report in last iteration, stop looping.");
                return;
            }
        }
    }
}
