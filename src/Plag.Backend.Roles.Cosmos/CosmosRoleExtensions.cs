#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Plag.Backend
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
    }
}
