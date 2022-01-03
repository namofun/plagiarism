#nullable enable

using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend
{
    internal static class CosmosQueryExtensions
    {
        private static async Task<List<T>> ToListAsync<T>(this FeedIterator<T> iterator, CancellationToken cancellationToken = default)
        {
            List<T> result = new();
            while (iterator.HasMoreResults)
            {
                foreach (T item in await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static async Task<List<TEntity>> GetListInternalAsync<TEntity>(this Container coll, QueryDefinition sql)
        {
            Stopwatch timer = Stopwatch.StartNew();
            List<TEntity> result;
            try
            {
                using FeedIterator<TEntity> iterator = coll.GetItemQueryIterator<TEntity>(sql);
                result = await iterator.ToListAsync();
            }
            catch (CosmosException ex)
            {
                timer.Stop();
                // _logger.LogQueryFailure(ex, coll, timer, sql);
                throw;
            }

            timer.Stop();
            // _logger.LogQuery(coll, timer, result.Count, sql);
            return result;
        }

        public static Task<List<TEntity>> GetListAsync<TEntity>(this Container coll, QueryDefinition sql) where TEntity : class
        {
            return GetListInternalAsync<TEntity>(coll, sql);
        }

        public static Task<List<TEntity>> GetListAsync<TEntity>(this Container coll, string sql) where TEntity : class
        {
            return GetListAsync<TEntity>(coll, new QueryDefinition(sql));
        }

        public static Task<List<TEntity>> GetListAsync<TEntity>(this Container coll, string sql, object param) where TEntity : class
        {
            QueryDefinition queryDefinition = new(sql);
            foreach ((string paramName, JToken? paramValue) in JObject.FromObject(param))
            {
                queryDefinition.WithParameter("@" + paramName, paramValue);
            }

            return GetListAsync<TEntity>(coll, queryDefinition);
        }

        public static async Task<TEntity?> GetEntityAsync<TEntity>(this Container coll, string id, string partitionKey) where TEntity : class
        {
            try
            {
                return await coll.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
