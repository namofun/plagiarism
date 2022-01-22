using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
            string rescueRecord =
                await Jobs.RescueWorker.RunAsync(
                    new AsyncCollectorSignalBroker(submissionTokenizer),
                    new AsyncCollectorSignalBroker(reportGenerator),
                    log);

            return new OkObjectResult(new { status = 202, comment = "Rescue request received.", trace = rescueRecord });
        }
    }
}
