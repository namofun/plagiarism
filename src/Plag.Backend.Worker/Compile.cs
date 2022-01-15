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
        private readonly IJobContext _store;
        private readonly SubmissionTokenizer _tokenizer;

        public Compile(IJobContext store, IConvertService2 converter, ICompileService compiler)
        {
            _store = store;
            _tokenizer = new(converter, compiler);
        }

        [FunctionName("Compile")]
        public async Task Run(
            [QueueTrigger(Constants.CompilationQueue, Connection = "AzureWebJobsStorage")] string queueMessage,
            [Queue(Constants.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> compilationContinuation,
            [Queue(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log)
        {
            string queueStamp = queueMessage;
            if (queueStamp.StartsWith("continuation"))
            {
                queueStamp = queueStamp.Split('%')[0];
            }

            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMinutes(9));

            log.LogInformation("Compilation worker started on {StartTime} for '{QueueMessage}'.", DateTimeOffset.Now, queueMessage);

            Submission next = null;
            int scheduleCounter = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                next = await _tokenizer.DoWorkAsync(_store);
                if (next == null) break;
                scheduleCounter++;

                log.LogDebug("Compilation finished for {SubmitId}.", next.ExternalId);

                if (next.TokenProduced == true)
                {
                    await _store.ScheduleAsync(next);
                    scheduleCounter++;
                }

                if (scheduleCounter >= 10)
                {
                    long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    await reportGenerator.AddAsync($"schedule|{timestamp}|{next.ExternalId}%{queueStamp}");
                    await reportGenerator.FlushAsync();
                    scheduleCounter = 0;
                }
            }

            if (scheduleCounter > 0)
            {
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                await reportGenerator.AddAsync($"schedule|{timestamp}|{Guid.Empty}%{queueStamp}");
                await reportGenerator.FlushAsync();
            }

            if (next != null)
            {
                // There is an active work item last iteration, but worker stopped here.
                // This happens when job is running out of time.

                Guid continuationId = Guid.NewGuid();
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string continuationRecord = $"continuation|{timestamp}|{continuationId}%{queueStamp}";
                await compilationContinuation.AddAsync(continuationRecord);
                await compilationContinuation.FlushAsync();
                log.LogInformation("Compilation continuation emitted with '{ContinuationId}'.", continuationRecord);
            }
            else
            {
                log.LogInformation("Compilation worker didn't get any submission in last iteration, stop looping.");
                return;
            }
        }
    }
}
