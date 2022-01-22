using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public static class RescueWorker
    {
        public static async Task<string> RunAsync(
            ISignalBroker submissionTokenizer,
            ISignalBroker reportGenerator,
            ILogger log)
        {
            string rescueRecord = CorrelationRecord.New("rescue");

            log.LogInformation("Rescue request received, enqueue '{RescueId}'.", rescueRecord);
            await submissionTokenizer.FireAsync(rescueRecord);
            await reportGenerator.FireAsync(rescueRecord);

            log.LogInformation("Wait for jobs to run from queue trigger.");
            return rescueRecord;
        }
    }
}
