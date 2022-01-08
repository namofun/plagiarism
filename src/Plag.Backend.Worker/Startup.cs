using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Services;
using System.Linq;

[assembly: FunctionsStartup(typeof(Plag.Backend.Worker.Startup))]

namespace Plag.Backend.Worker
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICompileService, AntlrCompileService>();
            builder.Services.AddSingleton<IConvertService2, DefaultConvertService2>();
            builder.Services.AddSingleton<ICosmosConnection, QueryProvider.CosmosConnection>();
            builder.Services.AddScoped<IJobContext, CosmosStoreService>();

            builder.Services.AddOptions<PlagBackendCosmosOptions>()
                .Configure<ICompileService, IConfiguration>((options, compiler, configuration) =>
                {
                    options.ConnectionString = configuration.GetConnectionString("CosmosDbAccount");
                    options.DatabaseName = configuration.GetConnectionString("CosmosDbName");

                    options.LanguageSeeds =
                        compiler.GetLanguages()
                            .Select(l => new Models.LanguageInfo()
                            {
                                Name = l.Name,
                                ShortName = l.ShortName,
                                Suffixes = l.Suffixes
                            })
                            .ToList();
                });
        }
    }
}
