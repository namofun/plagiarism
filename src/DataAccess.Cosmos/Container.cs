﻿using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Xylab.DataAccess.Cosmos
{
    public class Container<T> where T : class
    {
        private readonly QueryProvider _coll;

        internal Container(QueryProvider query)
        {
            _coll = query;
        }

        public Container<TEntity> AsType<TEntity>()
            where TEntity : class, T
        {
            return new Container<TEntity>(_coll);
        }

        public Task<TOutput> ExecuteStoredProcedureAsync<TOutput>(
            string storedProcedureId,
            PartitionKey partitionKey,
            dynamic[] parameters,
            StoredProcedureRequestOptions? requestOptions = null)
        {
            return _coll.Query<TOutput, StoredProcedureExecuteResponse<TOutput>>(
                "SPROC",
                $"EXEC {storedProcedureId} ({string.Join(", ", parameters.Select((a, i) => "@p" + i))}) OVER {partitionKey}",
                coll => coll.Scripts.ExecuteStoredProcedureAsync<TOutput>(
                    storedProcedureId,
                    partitionKey,
                    parameters,
                    requestOptions));
        }

        public Task UpsertAsync(T entity, PartitionKey partitionKey)
        {
            return _coll.Query<T, ItemResponse<T>>(
                "UPSERT",
                $"UPSERT OVER {partitionKey}",
                coll => coll.UpsertItemAsync(
                    entity,
                    partitionKey,
                    new ItemRequestOptions { EnableContentResponseOnWrite = false }));
        }

        public Task<T> CreateAsync(T entity, PartitionKey partitionKey)
        {
            return _coll.Query<T, ItemResponse<T>>(
                "CREATE",
                $"CREATE OVER {partitionKey}",
                coll => coll.CreateItemAsync(entity, partitionKey));
        }

        public Patch<T> Patch(string id, PartitionKey partitionKey)
        {
            return new(_coll, id, partitionKey);
        }

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql) where TEntity : class
        {
            return (await QueryAsync<TEntity>(sql).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql, PartitionKey? partitionKey) where TEntity : class
        {
            return (await QueryAsync<TEntity>(sql, partitionKey).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(string sql, object param, PartitionKey partitionKey) where TEntity : class
        {
            return (await QueryAsync<TEntity>(sql, param, partitionKey).ConfigureAwait(false)).SingleOrDefault();
        }

        public Task<List<TEntity>> QueryAsync<TEntity>(QueryDefinition sql, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return _coll.Query<TEntity>(sql, partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> QueryAsync<TEntity>(string sql, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            return _coll.Query<TEntity>(new QueryDefinition(sql), partitionKey, cancellationToken);
        }

        public Task<List<TEntity>> QueryAsync<TEntity>(string sql, object param, PartitionKey? partitionKey = default, CancellationToken cancellationToken = default) where TEntity : class
        {
            QueryDefinition queryDefinition = new(sql);
            foreach ((string paramName, JToken? paramValue) in JObject.FromObject(param))
            {
                queryDefinition.WithParameter("@" + paramName, paramValue);
            }

            return _coll.Query<TEntity>(queryDefinition, partitionKey, cancellationToken);
        }

        public async Task<TEntity?> FindAsync<TEntity>(string id, PartitionKey partitionKey) where TEntity : class
        {
            try
            {
                return await _coll.Query<TEntity, ItemResponse<TEntity>>(
                    "READ",
                    $"READ FOR @id OVER {partitionKey}",
                    coll => coll.ReadItemAsync<TEntity>(id, partitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task<T?> FindAsync(string id, PartitionKey partitionKey)
        {
            return FindAsync<T>(id, partitionKey);
        }

        public Task ReplaceAsync(T entity, string id, PartitionKey partitionKey, Azure.ETag? etag = default)
        {
            return _coll.Query<T, ItemResponse<T>>(
                "REPLACE",
                $"REPLACE FOR @id OVER {partitionKey}",
                coll => coll.ReplaceItemAsync(
                    entity,
                    id,
                    partitionKey,
                    new() { IfMatchEtag = etag?.ToString(), EnableContentResponseOnWrite = false }));
        }

        public Task<T> DeleteAsync(string id, PartitionKey partitionKey, Azure.ETag? etag = default)
        {
            return _coll.Query<T, ItemResponse<T>>(
                "DELETE",
                $"DELETE FOR @id OVER {partitionKey}",
                coll => coll.DeleteItemAsync<T>(
                    id,
                    partitionKey,
                    new() { IfMatchEtag = etag?.ToString(), EnableContentResponseOnWrite = true }));
        }

        public Task<IEnumerable<T>> FindAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> keys)
        {
            return _coll.Query<IEnumerable<T>, FeedResponse<T>>(
                "READ",
                $"READ FOR({keys.Count}) @id OVER @partitionKey",
                coll => coll.ReadManyItemsAsync<T>(keys));
        }

        public async Task BatchAsync<TModel>(
            string partitionKey,
            IEnumerable<TModel> source,
            Action<TModel, Batch<T>> batchEntryBuilder,
            Func<string, TModel[], TransactionalBatchResponse, Task>? postBatchResponse,
            int batchSize = 50,
            bool allowTooManyRequests = false,
            bool transactional = true)
        {
            if (batchSize > 100) throw new ArgumentOutOfRangeException(nameof(batchSize));
            foreach (TModel[] batchModel in source.Chunk(batchSize))
            {
                Batch<T> batch = _coll.CreateBatch<T>(new PartitionKey(partitionKey));
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
            Action<TModel, Batch<T>> batchEntryBuilder,
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
            Action<TModel, Batch<T>> batchEntryBuilder,
            Action<string, TModel[], TransactionalBatchResponse>? postBatchResponse = null,
            int batchSize = 50,
            bool allowTooManyRequests = false,
            bool transactional = true)
            => BatchAsync(
                partitionKey,
                source,
                batchEntryBuilder,
                postBatchResponse == null ? null : (a, b, c) => { postBatchResponse.Invoke(a, b, c); return Task.CompletedTask; },
                batchSize,
                allowTooManyRequests,
                transactional);

        public async Task BatchWithRetryAsync<TModel>(
            IEnumerable<TModel> source,
            Func<TModel, string> partitionKeySelector,
            Action<TModel, Batch<T>> batchEntryBuilder,
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
            Action<TModel, Batch<T>> batchEntryBuilder,
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
            Action<TModel, Batch<T>> batchEntryBuilder,
            Action<string, IEnumerable<(TModel, TransactionalBatchOperationResult)>>? postBatchResponse = null,
            int batchSize = 50)
            => BatchWithRetryAsync(
                partitionKey,
                source,
                batchEntryBuilder,
                postBatchResponse == null ? null : (a, b) => { postBatchResponse.Invoke(a, b); return Task.CompletedTask; },
                batchSize);
    }
}
