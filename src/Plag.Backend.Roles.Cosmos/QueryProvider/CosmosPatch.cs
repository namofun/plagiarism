using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Plag.Backend.QueryProvider
{
    public sealed class CosmosPatch<TEntity> : CosmosQueryProvider<TEntity>
    {
        private readonly string _id;
        private readonly List<PatchOperation> _operations;
        private string _concurrencyGuard;
        private bool _ignorePreconditionFailure;

        public CosmosPatch(Container container, string id, PartitionKey partitionKey, ILogger logger)
            : base(container, partitionKey, logger)
        {
            _id = id;
            _operations = new();
        }

        public CosmosPatch<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty propertyValue)
        {
            string path = "/" + ParseProperty(propertySelector);
            _operations.Add(PatchOperation.Replace(path, propertyValue));
            return this;
        }

        public CosmosPatch<TEntity> IncrementProperty(
            Expression<Func<TEntity, int>> propertySelector,
            int propertyValue)
        {
            string path = "/" + ParseProperty(propertySelector);
            _operations.Add(PatchOperation.Increment(path, propertyValue));
            return this;
        }

        public CosmosPatch<TEntity> ConcurrencyGuard(string conditionWithWhere)
        {
            _concurrencyGuard = conditionWithWhere;
            _ignorePreconditionFailure = false;
            return this;
        }

        public CosmosPatch<TEntity> UpdateOnCondition(string conditionWithWhere)
        {
            _concurrencyGuard = conditionWithWhere;
            _ignorePreconditionFailure = true;
            return this;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await _container.PatchItemAsync<TEntity>(
                    _id,
                    _partitionKey,
                    _operations,
                    new PatchItemRequestOptions()
                    {
                        EnableContentResponseOnWrite = false,
                        FilterPredicate = _concurrencyGuard,
                    });
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
    }
}
