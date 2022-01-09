using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Worker
{
    public class Bootstrap
    {
        private readonly ICosmosConnection _connection;
        private readonly ICompileService _compiler;

        public Bootstrap(ICosmosConnection connection, ICompileService compiler)
        {
            _connection = connection;
            _compiler = compiler;
        }

        [FunctionName("Bootstrap")]
        public async Task<IActionResult> Run(
            [HttpTrigger("post", Route = "bootstrap")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Start migrating data schema.");

            await _connection.MigrateAsync();

            log.LogInformation("Updating available language lists.");

            var languageSeeds = _compiler.GetLanguages()
                .Select(l => new LanguageInfo() { Name = l.Name, ShortName = l.ShortName, Suffixes = l.Suffixes })
                .ToList();

            await _connection.Metadata.UpsertAsync(new MetadataEntity<List<LanguageInfo>>()
            {
                Id = MetadataEntity.LanguagesMetadataKey,
                Type = MetadataEntity.SettingsTypeKey,
                Data = languageSeeds,
            });

            log.LogInformation("All metadata prepared.");
            return new OkObjectResult(new { status = 200, comment = "Bootstrapping finished." });
        }
    }
}
