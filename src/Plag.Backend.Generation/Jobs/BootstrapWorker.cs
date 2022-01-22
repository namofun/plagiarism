﻿using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public static class BootstrapWorker
    {
        public static async Task RunAsync(
            IJobContext store,
            ICompileService compiler,
            ILogger log)
        {
            log.LogInformation("Start migrating data schema.");

            await store.MigrateAsync();

            log.LogInformation("Updating available language lists.");

            var languageSeeds = compiler.GetLanguages()
                .Select(l => new LanguageInfo() { Name = l.Name, ShortName = l.ShortName, Suffixes = l.Suffixes })
                .ToList();

            await store.UpdateLanguagesAsync(languageSeeds);

            log.LogInformation("All metadata prepared.");
        }
    }
}
