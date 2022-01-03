using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Plag.Backend.Connectors;
using Plag.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public interface IPdsCosmosConnection
    {
        string SetsContainerName { get; }
        string SubmissionsContainerName { get; }
        string LanguagesContainerName { get; }
        string ReportsContainerName { get; }
        Database GetDatabase();
        void Migrate();
    }

    public class PdsCosmosConnection : IPdsCosmosConnection
    {
        private readonly PdsCosmosOptions _options;
        private readonly CosmosClient _client;

        public string SetsContainerName { get; } = "Sets";
        public string SubmissionsContainerName { get; } = "Submissions";
        public string LanguagesContainerName { get; } = "Languages";
        public string ReportsContainerName { get; } = "Reports";

        public PdsCosmosConnection(IOptions<PdsCosmosOptions> options)
        {
            _options = options.Value;

            _client = new CosmosClient(
                _options.ConnectionString,
                new CosmosClientOptions()
                {
                    Serializer = new PdsCosmosSerializer(_options.Serialization),
                    ApplicationName = "pds",
                });
        }

        public Database GetDatabase()
        {
            return _client.GetDatabase(_options.DatabaseName);
        }

        private async Task MigrateAsync()
        {
            await _client.CreateDatabaseIfNotExistsAsync(_options.DatabaseName).ConfigureAwait(false);
            Database database = _client.GetDatabase(_options.DatabaseName);

            Dictionary<string, string> pkeyPathMap = new()
            {
                [SetsContainerName] = "/id",
                [LanguagesContainerName] = "/id",
                [SubmissionsContainerName] = "/setid",
                [ReportsContainerName] = "/setid",
            };

            foreach ((string containerName, string partitionKeyPath) in pkeyPathMap)
            {
                await database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties()
                    {
                        Id = containerName,
                        PartitionKeyPath = partitionKeyPath,
                    });
            }

            if (_options.LanguageSeeds != null)
            {
                Container languages = database.GetContainer(LanguagesContainerName);
                foreach (LanguageInfo language in _options.LanguageSeeds)
                {
                    await languages.UpsertItemAsync(language).ConfigureAwait(false);
                }
            }
        }

        public void Migrate()
        {
            MigrateAsync().Wait();
        }
    }
}
