#nullable enable
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    public class CosmosContainer<T> where T : class
    {
        private readonly Container _coll;
        private readonly ILogger _logger;

        public CosmosContainer(Container container, ILogger logger)
        {
            _coll = container;
            _logger = logger;
        }

        public CosmosContainer<TEntity> AsType<TEntity>()
            where TEntity : class, T
        {
            return new(_coll, _logger);
        }

        public async Task<TOutput> ExecuteStoredProcedureAsync<TOutput>(
            string storedProcedureId,
            PartitionKey partitionKey,
            dynamic[] parameters,
            StoredProcedureRequestOptions? requestOptions = null)
        {
            return await _coll.Scripts.ExecuteStoredProcedureAsync<TOutput>(
                storedProcedureId,
                partitionKey,
                parameters,
                requestOptions);
        }

        public Task UpsertAsync(T entity, PartitionKey partitionKey)
        {
            return _coll.UpsertItemAsync(
                entity,
                partitionKey,
                new ItemRequestOptions { EnableContentResponseOnWrite = false });
        }

        public Task<ItemResponse<T>> CreateAsync(T entity, PartitionKey partitionKey)
        {
            return _coll.CreateItemAsync(entity, partitionKey);
        }

        public CosmosPatch<T> Patch(string id, PartitionKey partitionKey)
        {
            return new(_coll, id, partitionKey);
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

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql, PartitionKey? partitionKey) where TEntity : class
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

        public Task<T?> GetEntityAsync(string id, string partitionKey)
        {
            return GetEntityAsync<T>(id, partitionKey);
        }

        public Task<FeedResponse<T>> ReadManyItemsAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> keys)
        {
            return _coll.ReadManyItemsAsync<T>(keys);
        }

        public async Task BatchAsync<TModel>(
            string partitionKey,
            IEnumerable<TModel> source,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Func<string, TModel[], TransactionalBatchResponse, Task>? postBatchResponse,
            int batchSize = 50,
            bool allowTooManyRequests = false,
            bool transactional = true)
        {
            if (batchSize > 100) throw new ArgumentOutOfRangeException(nameof(batchSize));
            foreach (TModel[] batchModel in source.Chunk(batchSize))
            {
                CosmosBatch<T> batch = new(_coll.CreateTransactionalBatch(new(partitionKey)));
                foreach (TModel model in batchModel)
                {
                    batchEntryBuilder.Invoke(model, batch);
                }

                TransactionalBatchResponse resp = transactional
                    ? await batch.ExecuteAsync()
                    : await batch.ExecuteNonTransactionalAsync();

                if ((transactional
                        && !resp.IsSuccessStatusCode)
                    || (!transactional
                        && !resp.IsSuccessStatusCode
                        && resp.StatusCode != HttpStatusCode.PreconditionFailed
                        && !(resp.StatusCode == HttpStatusCode.TooManyRequests && allowTooManyRequests)))
                {
                    throw new CosmosException(
                        resp.ErrorMessage ?? $"Cosmos batch query failed with status code {resp.StatusCode}.",
                        resp.StatusCode,
                        0,
                        resp.ActivityId,
                        resp.RequestCharge);
                }

                if (postBatchResponse != null)
                {
                    await postBatchResponse.Invoke(partitionKey, batchModel, resp);
                }
            }
        }

        public async Task BatchAsync<TModel>(
            IEnumerable<TModel> source,
            Func<TModel, string> partitionKeySelector,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Action<string, TModel[], TransactionalBatchResponse>? postBatchResponse = null,
            int batchSize = 50,
            bool allowTooManyRequests = false,
            bool transactional = true)
        {
            foreach (IGrouping<string, TModel> partition in source.GroupBy(partitionKeySelector))
            {
                await BatchAsync(partition.Key, partition, batchEntryBuilder, postBatchResponse, batchSize, allowTooManyRequests, transactional);
            }
        }

        public Task BatchAsync<TModel>(
            string partitionKey,
            IEnumerable<TModel> source,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Action<string, TModel[], TransactionalBatchResponse>? postBatchResponse = null,
            int batchSize = 50,
            bool allowTooManyRequests = false,
            bool transactional = true)
            => BatchAsync(partitionKey, source, batchEntryBuilder, postBatchResponse.AsAsync(), batchSize, allowTooManyRequests, transactional);

        public async Task BatchWithRetryAsync<TModel>(
            IEnumerable<TModel> source,
            Func<TModel, string> partitionKeySelector,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Action<string, IEnumerable<(TModel, TransactionalBatchOperationResult)>>? postBatchResponse = null,
            int batchSize = 50)
        {
            List<TModel> sourceV2 = source.ToList();
            while (sourceV2.Count > 0)
            {
                List<TModel> staging = sourceV2.ToList();
                sourceV2.Clear();

                await BatchAsync(staging, partitionKeySelector, batchEntryBuilder, (pkey, models, resp) =>
                {
                    if (models.Length != resp.Count)
                    {
                        throw new InvalidOperationException("Batch operation must be one for one entity.");
                    }

                    sourceV2.AddRange(models.Zip(resp).Where(a => a.Second.StatusCode == HttpStatusCode.TooManyRequests).Select(a => a.First));
                    postBatchResponse?.Invoke(pkey, models.Zip(resp).Where(a => a.Second.StatusCode != HttpStatusCode.TooManyRequests));
                },
                batchSize: batchSize, allowTooManyRequests: true, transactional: false);
            }
        }

        public async Task BatchWithRetryAsync<TModel>(
            string partitionKey,
            IEnumerable<TModel> source,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Func<string, IEnumerable<(TModel, TransactionalBatchOperationResult)>, Task>? postBatchResponse,
            int batchSize = 50)
        {
            List<TModel> sourceV2 = source.ToList();
            while (sourceV2.Count > 0)
            {
                List<TModel> staging = sourceV2.ToList();
                sourceV2.Clear();

                await BatchAsync(partitionKey, staging, batchEntryBuilder, async (pkey, models, resp) =>
                {
                    if (models.Length != resp.Count)
                    {
                        throw new InvalidOperationException("Batch operation must be one for one entity.");
                    }

                    sourceV2.AddRange(models.Zip(resp).Where(a => a.Second.StatusCode == HttpStatusCode.TooManyRequests).Select(a => a.First));
                    if (postBatchResponse != null)
                    {
                        await postBatchResponse.Invoke(pkey, models.Zip(resp).Where(a => a.Second.StatusCode != HttpStatusCode.TooManyRequests));
                    }
                },
                batchSize: batchSize, allowTooManyRequests: true, transactional: false);
            }
        }

        public Task BatchWithRetryAsync<TModel>(
            string partitionKey,
            IEnumerable<TModel> source,
            Action<TModel, CosmosBatch<T>> batchEntryBuilder,
            Action<string, IEnumerable<(TModel, TransactionalBatchOperationResult)>>? postBatchResponse = null,
            int batchSize = 50)
            => BatchWithRetryAsync(partitionKey, source, batchEntryBuilder, postBatchResponse.AsAsync(), batchSize);
    }
}
