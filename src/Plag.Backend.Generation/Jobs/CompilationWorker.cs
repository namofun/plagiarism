using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend.Jobs
{
    public static class CompilationWorker
    {
        public static async Task RunAsync(
            string queueMessage,
            IJobContext store,
            IConvertService2 converter,
            ICompileService compiler,
            ILogger log,
            ISignalBroker compilationContinuation,
            ISignalBroker reportGenerator,
            ITelemetryClient telemetryClient,
            CancellationToken cancellationToken)
        {
            string queueStamp = CorrelationRecord.Parent(queueMessage);
            SubmissionTokenizer tokenizer = new(converter, compiler, telemetryClient);

            using CancellationTokenSource cts = new();
            using CancellationTokenSource ctsV2 = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(9));

            log.LogInformation("Compilation worker started on {StartTime} for '{QueueMessage}'.", DateTimeOffset.Now, queueMessage);

            Submission next = null;
            int scheduleCounter = 0;
            while (!ctsV2.Token.IsCancellationRequested)
            {
                next = await telemetryClient.TrackScope("Compile.Peek", () => tokenizer.DoWorkAsync(store));
                if (next == null) break;
                scheduleCounter++;

                log.LogDebug("Compilation finished for {SubmitId}.", next.ExternalId);

                if (next.TokenProduced == true)
                {
                    await telemetryClient.TrackScope("Compile.Schedule", () => store.ScheduleAsync(next));
                    scheduleCounter++;
                }

                if (scheduleCounter >= 10)
                {
                    string record = CorrelationRecord.New("schedule", queueStamp);
                    log.LogInformation("Compiled around 10 submissions, starting report worker {record}.", record);
                    await reportGenerator.FireAsync(record);
                    scheduleCounter = 0;
                }
            }

            if (scheduleCounter > 0)
            {
                string record = CorrelationRecord.New("schedule", queueStamp);
                log.LogInformation("Compiled around 10 submissions, starting report worker {record}.", record);
                await reportGenerator.FireAsync(record);
            }

            if (next != null)
            {
                // There is an active work item last iteration, but worker stopped here.
                // This happens when job is running out of time.
                string record = CorrelationRecord.New("continuation", queueStamp);
                await compilationContinuation.FireAsync(record);
                log.LogInformation("Compilation continuation emitted with '{ContinuationId}'.", record);
            }
            else
            {
                log.LogInformation("Compilation worker didn't get any submission in last iteration, stop looping.");
            }
        }
    }
}
