using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Plag.Backend.Worker
{
    public class Rescue
    {
        [FunctionName("Rescue")]
        public async Task<IActionResult> Run(
            [HttpTrigger("post", Route = "rescue")] HttpRequest req,
            [Queue(Constants.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            [Queue(Constants.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log)
        {
            Guid rescueId = Guid.NewGuid();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string rescueRecord = $"rescue|{timestamp}|{rescueId}";

            log.LogInformation("Rescue request received, enqueue '{RescueId}'.", rescueRecord);
            await submissionTokenizer.AddAsync(rescueRecord);
            await reportGenerator.AddAsync(rescueRecord);
            await submissionTokenizer.FlushAsync();
            await reportGenerator.FlushAsync();

            log.LogInformation("Wait for jobs to run from queue trigger.");
            return new OkObjectResult(new { status = 202, comment = "Rescue request received.", trace = rescueRecord });
        }
    }
}
