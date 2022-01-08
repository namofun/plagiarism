#nullable enable

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Plag.Backend.QueryProvider
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly PlagBackendCosmosOptions _options;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosConnection> _logger;
        private readonly Database _database;

        public CosmosContainer<SetEntity> Sets { get; }
        public CosmosContainer<SubmissionEntity> Submissions { get; }
        public CosmosContainer<ReportEntity> Reports { get; }
        public CosmosContainer<MetadataEntity> Metadata { get; }

        public CosmosConnection(IOptions<PlagBackendCosmosOptions> options, ILogger<CosmosConnection> logger)
        {
            _options = options.Value;
            _logger = logger;

            _client = new CosmosClient(
                _options.ConnectionString,
                new CosmosClientOptions()
                {
                    Serializer = new HybridCosmosSerializer(_options.Serialization),
                    ApplicationName = "pds",
                });

            _database = _client.GetDatabase(_options.DatabaseName);
            Sets = new(_database.GetContainer(nameof(Metadata)), logger);
            Submissions = new(_database.GetContainer(nameof(Submissions)), logger);
            Reports = new(_database.GetContainer(nameof(Reports)), logger);
            Metadata = new(_database.GetContainer(nameof(Metadata)), logger);
        }

        public Database GetDatabase()
        {
            return _database;
        }

        public async Task MigrateAsync()
        {
            DatabaseResponse databaseResponse = await _client
                .CreateDatabaseIfNotExistsAsync(_options.DatabaseName)
                .ConfigureAwait(false);

            if (databaseResponse.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Database {Name} created successfully.", _options.DatabaseName);
            }

            Database database = databaseResponse.Database;

            Dictionary<string, string> pkeyPathMap = new()
            {
                [nameof(Metadata)] = "/type",
                [nameof(Submissions)] = "/setid",
                [nameof(Reports)] = "/setid",
            };

            foreach ((string containerName, string partitionKeyPath) in pkeyPathMap)
            {
                ContainerResponse containerResponse = await database
                    .CreateContainerIfNotExistsAsync(
                        new ContainerProperties()
                        {
                            Id = containerName,
                            PartitionKeyPath = partitionKeyPath,
                        });

                if (containerResponse.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Container {Name} created with pk '{PartitionKeyPath}' successfully.", containerName, partitionKeyPath);
                }
            }

            if (_options.LanguageSeeds != null)
            {
                await Metadata.UpsertAsync(new MetadataEntity<List<LanguageInfo>>()
                {
                    Id = MetadataEntity.LanguagesMetadataKey,
                    Type = MetadataEntity.SettingsTypeKey,
                    Data = _options.LanguageSeeds,
                });
            }
        }
    }
}
