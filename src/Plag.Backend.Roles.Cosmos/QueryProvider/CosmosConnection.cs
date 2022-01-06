#nullable enable

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plag.Backend.Connectors;
using Plag.Backend.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Plag.Backend.QueryProvider
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly PdsCosmosOptions _options;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosConnection> _logger;
        private readonly Database _database;

        public CosmosContainer<PlagiarismSet> Sets { get; }
        public CosmosContainer<Entities.SubmissionEntity> Submissions { get; }
        public CosmosContainer<LanguageInfo> Languages { get; }
        public CosmosContainer<Entities.ReportEntity> Reports { get; }

        public CosmosConnection(IOptions<PdsCosmosOptions> options, ILogger<CosmosConnection> logger)
        {
            _options = options.Value;
            _logger = logger;

            _client = new CosmosClient(
                _options.ConnectionString,
                new CosmosClientOptions()
                {
                    Serializer = new PdsCosmosSerializer(_options.Serialization),
                    ApplicationName = "pds",
                });

            _database = _client.GetDatabase(_options.DatabaseName);
            Sets = new(_database.GetContainer(nameof(Sets)), logger);
            Submissions = new(_database.GetContainer(nameof(Submissions)), logger);
            Languages = new(_database.GetContainer(nameof(Languages)), logger);
            Reports = new(_database.GetContainer(nameof(Reports)), logger);
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
                [nameof(Sets)] = "/id",
                [nameof(Languages)] = "/id",
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
                foreach (LanguageInfo language in _options.LanguageSeeds)
                {
                    await Languages.UpsertAsync(language).ConfigureAwait(false);
                }
            }
        }
    }
}
