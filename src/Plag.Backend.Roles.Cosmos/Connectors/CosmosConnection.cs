#nullable enable

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Plag.Backend.Connectors;
using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public interface ICosmosConnection
    {
        CosmosContainer Sets { get; }
        CosmosContainer Submissions { get; }
        CosmosContainer Languages { get; }
        CosmosContainer Reports { get; }

        Database GetDatabase();
        Task MigrateAsync();
    }

    public class CosmosContainer
    {
        private readonly Container _coll;
        private readonly ILogger _logger;

        public CosmosContainer(Container container, ILogger logger)
        {
            _coll = container;
            _logger = logger;
        }

        public Container GetContainer()
        {
            return _coll;
        }

        private async Task<List<TEntity>> GetListInternalAsync<TEntity>(QueryDefinition sql, string? partitionKey, CancellationToken cancellationToken)
        {
            Stopwatch timer = Stopwatch.StartNew();
            EventId eventId = new(10060, "CosmosDbQuery");
            List<TEntity> result = new();
            QueryRequestOptions? options = partitionKey == null ? null : new() { PartitionKey = new PartitionKey(partitionKey) };
            try
            {
                using FeedIterator<TEntity> iterator = _coll.GetItemQueryIterator<TEntity>(sql, requestOptions: options);
                while (iterator.HasMoreResults)
                {
                    foreach (TEntity item in await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        result.Add(item);
                    }
                }
            }
            catch (CosmosException ex)
            {
                timer.Stop();
                _logger.LogError(eventId, ex,
                    "Failed to query from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _coll.Id + (partitionKey == null ? "" : "(pk=\"" + partitionKey + ")"), timer.ElapsedMilliseconds, sql.QueryText);
                throw;
            }

            timer.Stop();
            _logger.LogInformation(eventId,
                "Queried from [{ContainerName}] within {ElapsedTime}ms, {Count} results.\r\n{QueryText}",
                _coll.Id + (partitionKey == null ? "" : "(pk=\"" + partitionKey + ")"), timer.ElapsedMilliseconds, result.Count, sql.QueryText);
            return result;
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(QueryDefinition sql, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return GetListInternalAsync<TEntity>(sql, partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(string sql, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return GetListInternalAsync<TEntity>(new QueryDefinition(sql), partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(string sql, object param, string? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            QueryDefinition queryDefinition = new(sql);
            foreach ((string paramName, JToken? paramValue) in JObject.FromObject(param))
            {
                queryDefinition.WithParameter("@" + paramName, paramValue);
            }

            return GetListInternalAsync<TEntity>(queryDefinition, partitionKey, cancellationToken);
        }

        public async Task<TEntity?> GetEntityAsync<TEntity>(string id, string partitionKey) where TEntity : class
        {
            try
            {
                return await _coll.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }

    public class CosmosConnection : ICosmosConnection
    {
        private readonly PdsCosmosOptions _options;
        private readonly CosmosClient _client;
        private readonly ILogger<CosmosConnection> _logger;
        private readonly Database _database;
        private readonly CosmosContainer _sets;
        private readonly CosmosContainer _submissions;
        private readonly CosmosContainer _languages;
        private readonly CosmosContainer _reports;

        public CosmosContainer Sets => _sets;
        public CosmosContainer Submissions => _submissions;
        public CosmosContainer Languages => _languages;
        public CosmosContainer Reports => _reports;

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
            _sets = new CosmosContainer(_database.GetContainer("Sets"), logger);
            _submissions = new CosmosContainer(_database.GetContainer("Submissions"), logger);
            _languages = new CosmosContainer(_database.GetContainer("Languages"), logger);
            _reports = new CosmosContainer(_database.GetContainer("Reports"), logger);
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
                Container languages = _languages.GetContainer();
                foreach (LanguageInfo language in _options.LanguageSeeds)
                {
                    await languages.UpsertItemAsync(language).ConfigureAwait(false);
                }
            }
        }
    }
}
