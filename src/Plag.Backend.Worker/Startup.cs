using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Plag.Backend.Jobs;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Plag.Backend.Worker.Startup))]

namespace Plag.Backend.Worker
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddPlagGenerationService();

            IConfiguration configuration = builder.GetContext().Configuration;
            if (configuration.GetConnectionString("Primary") == "SqlServer")
            {
                builder.Services.AddRelationalForPlagWorker<WorkerContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("SqlServer"), b => b.UseBulk());
                });
            }
            else
            {
                builder.Services.AddCosmosForPlagWorker(options =>
                {
                    options.ConnectionString = configuration.GetConnectionString("CosmosDbAccount");
                    options.DatabaseName = configuration.GetConnectionString("CosmosDbName");
                });
            }
        }

        private class WorkerContext : DbContext
        {
            public WorkerContext(DbContextOptions<WorkerContext> options)
                : base(options)
            {
            }
        }
    }

    internal class AsyncCollectorSignalBroker : ISignalBroker
    {
        private readonly IAsyncCollector<string> _asyncCollector;

        public AsyncCollectorSignalBroker(IAsyncCollector<string> asyncCollector)
        {
            _asyncCollector = asyncCollector;
        }

        public async Task FireAsync(string signal)
        {
            await _asyncCollector.AddAsync(signal);
            await _asyncCollector.FlushAsync();
        }
    }
}
