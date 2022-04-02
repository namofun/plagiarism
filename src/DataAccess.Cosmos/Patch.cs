using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Xylab.DataAccess.Cosmos
{
    internal static partial class InternalExtensions
    {
        public static void SetProperty<TEntity, TProperty>(
            QueryProvider queryProvider,
            List<PatchOperation> patch,
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty propertyValue)
        {
            string path = "/" + queryProvider.Options.ParseProperty(propertySelector);

            if (propertyValue == null)
            {
                patch.Add(PatchOperation.Set(path, JValue.CreateNull()));
            }
            else
            {
                patch.Add(PatchOperation.Set(path, propertyValue));
            }
        }

        public static void SetProperty<TEntity, TProperty>(
            QueryProvider queryProvider,
            List<PatchOperation> patch,
            Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
            string dictionaryKey,
            TProperty propertyValue)
        {
            string path = "/" + queryProvider.Options.ParseProperty(dictPropSelector) + "/" + dictionaryKey;

            if (propertyValue == null)
            {
                patch.Add(PatchOperation.Set(path, JValue.CreateNull()));
            }
            else
            {
                patch.Add(PatchOperation.Set(path, propertyValue));
            }
        }

        public static void IncrementProperty<TEntity>(
            QueryProvider queryProvider,
            List<PatchOperation> patch,
            Expression<Func<TEntity, int>> propertySelector,
            int propertyValue)
        {
            string path = "/" + queryProvider.Options.ParseProperty(propertySelector);
            patch.Add(PatchOperation.Increment(path, propertyValue));
        }

        public static void IncrementProperty<TEntity>(
            QueryProvider queryProvider,
            List<PatchOperation> patch,
            Expression<Func<TEntity, long>> propertySelector,
            long propertyValue)
        {
            string path = "/" + queryProvider.Options.ParseProperty(propertySelector);
            patch.Add(PatchOperation.Increment(path, propertyValue));
        }
    }

    public sealed class Patch<TEntity>
    {
        private readonly QueryProvider _container;
        private readonly PartitionKey _partitionKey;
        private readonly string _id;
        private readonly List<PatchOperation> _operations;
        private string? _concurrencyGuard;
        private bool _ignorePreconditionFailure;

        internal Patch(QueryProvider container, string id, PartitionKey partitionKey)
        {
            _container = container;
            _partitionKey = partitionKey;
            _id = id;
            _operations = new();
        }

        public Patch<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty propertyValue)
        {
            InternalExtensions.SetProperty(_container, _operations, propertySelector, propertyValue);
            return this;
        }

        public Patch<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
            string dictionaryKey,
            TProperty propertyValue)
        {
            InternalExtensions.SetProperty(_container, _operations, dictPropSelector, dictionaryKey, propertyValue);
            return this;
        }

        public Patch<TEntity> IncrementProperty(
            Expression<Func<TEntity, int>> propertySelector,
            int propertyValue)
        {
            InternalExtensions.IncrementProperty(_container, _operations, propertySelector, propertyValue);
            return this;
        }

        public Patch<TEntity> IncrementProperty(
            Expression<Func<TEntity, long>> propertySelector,
            long propertyValue)
        {
            InternalExtensions.IncrementProperty(_container, _operations, propertySelector, propertyValue);
            return this;
        }

        public Patch<TEntity> ConcurrencyGuard(string conditionWithWhere)
        {
            _concurrencyGuard = conditionWithWhere;
            _ignorePreconditionFailure = false;
            return this;
        }

        public Patch<TEntity> UpdateOnCondition(string conditionWithWhere)
        {
            _concurrencyGuard = conditionWithWhere;
            _ignorePreconditionFailure = true;
            return this;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await _container.Query<TEntity, ItemResponse<TEntity>>(
                    "PATCH",
                    $"PATCH {string.Join(", ", _operations.Select(o => $"{o.OperationType}(\"{o.Path}\")"))} OVER {_partitionKey}" + (_concurrencyGuard == null ? "" : "\r\n" + _concurrencyGuard),
                    coll => coll.PatchItemAsync<TEntity>(
                        _id,
                        _partitionKey,
                        _operations,
                        new PatchItemRequestOptions()
                        {
                            EnableContentResponseOnWrite = false,
                            FilterPredicate = _concurrencyGuard,
                        }));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Entity not found in set.", ex);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed && _ignorePreconditionFailure)
            {
                // Skip the failure when only update on condition
            }
        }

        public async Task ExecuteWithRetryAsync()
        {
            for (int tries = 0; ; tries++)
            {
                try
                {
                    await ExecuteAsync();
                    return;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (ex.RetryAfter.HasValue)
                    {
                        await Task.Delay(ex.RetryAfter.Value);
                    }
                    else if (tries >= 2)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
