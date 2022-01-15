using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.Linq;
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
            log.LogInformation("Start migrating data schema.");

            await _store.MigrateAsync();

            log.LogInformation("Updating available language lists.");

            var languageSeeds = _compiler.GetLanguages()
                .Select(l => new LanguageInfo() { Name = l.Name, ShortName = l.ShortName, Suffixes = l.Suffixes })
                .ToList();

            await _store.UpdateLanguagesAsync(languageSeeds);

            log.LogInformation("All metadata prepared.");
            return new OkObjectResult(new { status = 200, comment = "Bootstrapping finished." });
        }
    }
}
