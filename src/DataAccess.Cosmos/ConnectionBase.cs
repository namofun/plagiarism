using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace Xylab.DataAccess.Cosmos
{
    public abstract class ConnectionBase
    {
        private readonly CosmosOptions _options;
        private readonly CosmosClient _client;
        private readonly ILogger _logger;
        private readonly Database _database;
        private readonly ITelemetryClient _telemetryClient;

        protected ConnectionBase(
            string connectionString,
            string databaseName,
            CosmosOptions options,
            ILogger logger,
            ITelemetryClient telemetryClient)
        {
            _options = options;
            _logger = logger;
            _telemetryClient = telemetryClient;

            CosmosClientOptions clientOptions = new()
            {
                ApplicationName = "pds",
                Serializer = new HybridSerializer(new()
                {
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.None,
                    ContractResolver = new HybridContractResolver(options),
                }),
            };

            options.ConfigureClientOptions?.Invoke(clientOptions);
            _client = new CosmosClient(connectionString, clientOptions);
            _database = _client.GetDatabase(databaseName);
        }

        protected Container<TEntity> Container<TEntity>(string collectionName) where TEntity : class
        {
            return new Container<TEntity>(
                new QueryProvider(
                    _telemetryClient,
                    _logger,
                    _database.GetContainer(collectionName),
                    _options));
        }

        public Task MigrateAsync()
        {
            return _telemetryClient.TrackScope("Cosmos.Migration", MigrateAsyncCore);
        }

        private async Task MigrateAsyncCore()
        {
            DatabaseResponse databaseResponse = await _client
                .CreateDatabaseIfNotExistsAsync(_database.Id)
                .ConfigureAwait(false);

            if (databaseResponse.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Database {Name} created successfully.", _database.Id);
            }

            foreach ((string containerName, string partitionKeyPath) in _options.PartitionKeyMapping)
            {
                ContainerProperties props = new()
                {
                    Id = containerName,
                    PartitionKeyPath = partitionKeyPath,
                };

                if (_options.CustomIndexingPolicy.TryGetValue(containerName, out var indexPolicy))
                {
                    indexPolicy.Invoke(props.IndexingPolicy);
                }

                ContainerResponse containerResponse =
                    await _database.CreateContainerIfNotExistsAsync(props);
                if (containerResponse.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Container {Name} created with pk '{PartitionKeyPath}' successfully.", containerName, partitionKeyPath);
                }
            }

            foreach ((string name, var data) in _options.StoredProcedures)
            {
                Container container = _database.GetContainer(data.CollectionName);
                try
                {
                    await container.Scripts.CreateStoredProcedureAsync(
                        new StoredProcedureProperties()
                        {
                            Id = name,
                            Body = data.Code,
                        });
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                }
            }
        }
    }
}
