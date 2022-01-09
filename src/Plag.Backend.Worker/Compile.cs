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
    public class Compile
    {
        private readonly ICosmosConnection _connection;
        private readonly SubmissionTokenizer _tokenizer;

        public Compile(ICosmosConnection connection, IConvertService2 converter, ICompileService compiler)
        {
            _connection = connection;
            _tokenizer = new(converter, compiler);
        }

        [FunctionName("Compile")]
        public async Task Run(
            [QueueTrigger(Constants.CompilationQueue, Connection = "AzureWebJobsStorage")] string queueMessage,
            [Queue(Constants.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> compilationContinuation,
            [Queue(Constants.ReportSchedulingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportScheduler,
            ILogger log)
        {
            string queueStamp = queueMessage;
            if (queueStamp.StartsWith("continuation"))
            {
                queueStamp = queueStamp.Split('%')[0];
            }

            IJobContext store = new CosmosStoreService(_connection);
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMinutes(9));

            log.LogInformation("Worker started on {StartTime} for '{QueueMessage}'.", DateTimeOffset.Now, queueMessage);

            Submission next = null;
            int scheduleCounter = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                scheduleCounter++;
                next = await _tokenizer.DoWorkAsync(store);
                if (next == null) break;

                log.LogDebug("Compilation finished for {SubmitId}.", next.ExternalId);

                if (next.TokenProduced == true)
                {
                    await reportScheduler.AddAsync(
                        $"\\\\{next.SetId}" +
                        $"\\{next.Language}" +
                        $"\\{next.InclusiveCategory}" +
                        $"\\{next.ExclusiveCategory}" +
                        $"\\{next.ExternalId}");
                }

                if (scheduleCounter % 10 == 0)
                {
                    await reportScheduler.FlushAsync();
                }
            }

            await reportScheduler.FlushAsync();

            if (next != null)
            {
                // There is an active work item last iteration, but worker stopped here.
                // This happens when job is running out of time.

                Guid continuationId = Guid.NewGuid();
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string continuationRecord = $"continuation|{timestamp}|{continuationId}%{queueStamp}";
                await compilationContinuation.AddAsync(continuationRecord);
                await compilationContinuation.FlushAsync();
                log.LogInformation("Continuation emitted with '{ContinuationId}'.", continuationRecord);
            }
            else
            {
                log.LogInformation("Worker didn't get any submission in last iteration, stop looping.");
                return;
            }
        }
    }
}
