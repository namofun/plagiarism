using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public static class ReportWorker
    {
        public static async Task RunAsync(
            string signal,
            IJobContext store,
            IConvertService2 converter,
            ICompileService compiler,
            IReportService reporter,
            ILogger log,
            ISignalBroker reportContinuation,
            CancellationToken cancellationToken)
        {
            string queueStamp = CorrelationRecord.Parent(signal);
            ReportGenerator gen = new(compiler, converter, reporter);
            gen.SetLogger(log);

            var lru = new LruStore<(string, int), (Submission, Frontend.Submission)>();
            using CancellationTokenSource cts = new();
            using CancellationTokenSource ctsV2 = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(9));

            log.LogInformation("Report worker started on {StartTime} for '{QueueMessage}'.", DateTimeOffset.Now, signal);

            int lastBatch = 0;
            while (!ctsV2.Token.IsCancellationRequested)
            {
                lastBatch = await gen.DoWorkBatchAsync(store, lru);
                if (lastBatch == 0) break;
                log.LogDebug("{count} reports are finished and saved.", lastBatch);
            }

            lru.Clear();
            if (lastBatch > 0)
            {
                // There is an active work item last iteration, but worker stopped here.
                // This happens when job is running out of time.
                string continuationRecord = CorrelationRecord.New("continuation", queueStamp);
                await reportContinuation.FireAsync(continuationRecord);
                log.LogInformation("Report continuation emitted with '{ContinuationId}'.", continuationRecord);
            }
            else
            {
                log.LogInformation("Report worker didn't pick up any report in last iteration, stop looping.");
            }
        }
    }
}
