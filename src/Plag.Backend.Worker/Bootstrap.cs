using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Services;
using System.Threading.Tasks;

namespace Plag.Backend.Worker
{
    public class Bootstrap
    {
        private readonly IJobContext _store;
        private readonly ICompileService _compiler;

        public Bootstrap(IJobContext store, ICompileService compiler)
        {
            _store = store;
            _compiler = compiler;
        }

        [FunctionName("Bootstrap")]
        public async Task<IActionResult> Run(
            [HttpTrigger("post", Route = "bootstrap")] HttpRequest req,
            ILogger log)
        {
            await Jobs.BootstrapWorker.RunAsync(_store, _compiler, log);
            return new OkObjectResult(new { status = 200, comment = "Bootstrapping finished." });
        }
    }
}
