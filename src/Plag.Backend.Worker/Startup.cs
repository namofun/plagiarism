using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
}
