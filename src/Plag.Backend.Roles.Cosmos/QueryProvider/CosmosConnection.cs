#nullable enable
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Entities;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly PlagBackendCosmosOptions _options;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosConnection> _logger;
        private readonly Database _database;
        private readonly ITelemetryClient _telemetryClient;

        public CosmosContainer<SetEntity> Sets { get; }
        public CosmosContainer<SubmissionEntity> Submissions { get; }
        public CosmosContainer<ReportEntity> Reports { get; }
        public CosmosContainer<MetadataEntity> Metadata { get; }

        public CosmosConnection(
            IOptions<PlagBackendCosmosOptions> options,
            ILogger<CosmosConnection> logger,
            ITelemetryClient telemetryClient)
        {
            _options = options.Value;
            _logger = logger;
            _telemetryClient = telemetryClient;

            _client = new CosmosClient(
                _options.ConnectionString,
                new CosmosClientOptions()
                {
                    Serializer = new HybridCosmosSerializer(_options.Serialization),
                    ApplicationName = "pds",
                });

            _database = _client.GetDatabase(_options.DatabaseName);
            Sets = new(_database.GetContainer(nameof(Metadata)), logger, _telemetryClient);
            Submissions = new(_database.GetContainer(nameof(Submissions)), logger, _telemetryClient);
            Reports = new(_database.GetContainer(nameof(Reports)), logger, _telemetryClient);
            Metadata = new(_database.GetContainer(nameof(Metadata)), logger, _telemetryClient);
        }

        public Task MigrateAsync()
        {
            return _telemetryClient.TrackScope("Cosmos.Migration", MigrateAsyncCore);
        }

        private async Task MigrateAsyncCore()
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

            List<KeyValuePair<string, string>> sprocs = new()
            {
                new(CosmosContainerStoredProcedureExtensions.QueryServiceGraph, nameof(Metadata)),
            };

            foreach ((string name, string containerName) in sprocs)
            {
                string code = await GetStoredProcedureCodeAsync(name);
                Container container = database.GetContainer(containerName);
                try
                {
                    await container.Scripts.CreateStoredProcedureAsync(
                        new StoredProcedureProperties()
                        {
                            Id = name,
                            Body = code,
                        });
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                }
            }
        }

        private static async Task<string> GetStoredProcedureCodeAsync(string name)
        {
            string sproc = $"Xylab.PlagiarismDetect.Backend.QueryProvider.{name}.js";
            using Stream stream = typeof(CosmosConnection).Assembly.GetManifestResourceStream(sproc)
                ?? throw new InvalidDataException();

            using StreamReader sr = new(stream);
            return await sr.ReadToEndAsync();
        }
    }

    public static class CosmosContainerStoredProcedureExtensions
    {
        internal const string QueryServiceGraph = nameof(QueryServiceGraph);

        public static Task<List<Models.ServiceVertex>> QueryServiceGraphAsync(
            this CosmosContainer<MetadataEntity> container,
            SetGuid setId,
            string language,
            int inclusiveCategory,
            int exclusiveCategory)
        {
            return container.ExecuteStoredProcedureAsync<List<Models.ServiceVertex>>(
                QueryServiceGraph,
                new PartitionKey(MetadataEntity.ServiceGraphTypeKey),
                new object[] { setId.ToString(), language, inclusiveCategory, exclusiveCategory },
                new() { EnableScriptLogging = false });
        }
    }
}
