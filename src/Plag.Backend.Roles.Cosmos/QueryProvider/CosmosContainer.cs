#nullable enable

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.QueryProvider
{
    public class CosmosContainer<T>
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

        public Task UpsertAsync(T entity)
        {
            return _coll.UpsertItemAsync(
                entity,
                default(PartitionKey?),
                new ItemRequestOptions { EnableContentResponseOnWrite = false });
        }

        public Task<ItemResponse<T>> CreateAsync(T entity)
        {
            return _coll.CreateItemAsync(entity);
        }

        public CosmosPatch<T> Patch(string id, PartitionKey partitionKey)
        {
            return new(_coll, id, partitionKey, _logger);
        }

        private async Task<List<TEntity>> GetListInternalAsync<TEntity>(QueryDefinition sql, PartitionKey? partitionKey, CancellationToken cancellationToken)
        {
            Stopwatch timer = Stopwatch.StartNew();
            EventId eventId = new(10060, "CosmosDbQuery");
            List<TEntity> result = new();
            QueryRequestOptions options = new() { PartitionKey = partitionKey };
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
                    _coll.Id + (partitionKey == null ? "" : "(pk=" + partitionKey + ")"), timer.ElapsedMilliseconds, sql.QueryText);
                throw;
            }

            timer.Stop();
            _logger.LogInformation(eventId,
                "Queried from [{ContainerName}] within {ElapsedTime}ms, {Count} results.\r\n{QueryText}",
                _coll.Id + (partitionKey == null ? "" : "(pk=" + partitionKey + ")"), timer.ElapsedMilliseconds, result.Count, sql.QueryText);
            return result;
        }

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql, PartitionKey partitionKey) where TEntity : class
        {
            return (await GetListAsync<TEntity>(sql, partitionKey).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql, object param, PartitionKey partitionKey) where TEntity : class
        {
            return (await GetListAsync<TEntity>(sql, param, partitionKey).ConfigureAwait(false)).SingleOrDefault();
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(QueryDefinition sql, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return GetListInternalAsync<TEntity>(sql, partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(string sql, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return GetListInternalAsync<TEntity>(new QueryDefinition(sql), partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> GetListAsync<TEntity>(string sql, object param, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
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
}
