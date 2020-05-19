using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SatelliteSite.Data
{
    public static class RepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the context to connect to a Azure Cosmos Database
        /// </summary>
        /// <param name="builder">Context builder</param>
        /// <param name="configuration">Keys to the access token { "AccountEndpoint" : "...", "PrimaryKey" : "..." }</param>
        /// <param name="databaseName">Database name</param>
        /// <returns>The previous options builder</returns>
        public static DbContextOptionsBuilder UseCosmos(
            this DbContextOptionsBuilder builder,
            IConfiguration configuration,
            string databaseName)
        {
            return builder.UseCosmos(
                accountEndpoint: configuration["AccountEndpoint"],
                accountKey: configuration["PrimaryKey"],
                databaseName: databaseName);
        }
    }
}
