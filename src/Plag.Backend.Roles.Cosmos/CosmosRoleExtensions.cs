#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend
{
    public static class CosmosRoleExtensions
    {
        /// <summary>
        /// Configures the Cosmos DB connection with instance of <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="host">The host builder.</param>
        /// <param name="configureOptions">The options configure.</param>
        /// <returns>The host builder to chain calls.</returns>
        public static IHostBuilder AddCosmos(
            this IHostBuilder host,
            Action<HostBuilderContext, PlagBackendCosmosOptions> configureOptions)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.Configure<PlagBackendCosmosOptions>(options =>
                {
                    configureOptions.Invoke(context, options);
                });
            });
        }

        /// <summary>
        /// Configures the cosmos db role for functional worker.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The connection configurator.</param>
        public static void AddCosmosForPlagWorker(
            this IServiceCollection services,
            Action<PlagBackendCosmosOptions> configureOptions)
        {

            services.AddSingleton<ICosmosConnection, QueryProvider.CosmosConnection>();
            services.AddSingleton<ISignalProvider, NullSignalProvider>();
            services.AddScoped<IJobContext, CosmosStoreService>();
            services.Configure<PlagBackendCosmosOptions>(configureOptions);
        }
    }
}
