using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    internal static class CosmosPatchBuilderExtensions
    {
        public static void SetProperty<TEntity, TProperty>(
            this List<PatchOperation> patch,
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty propertyValue)
        {
            string path = "/" + propertySelector.ParseProperty();

            if (propertyValue == null)
            {
                patch.Add(PatchOperation.Replace(path, JValue.CreateNull()));
            }
            else
            {
                patch.Add(PatchOperation.Replace(path, propertyValue));
            }
        }

        public static void SetProperty<TEntity, TProperty>(
            this List<PatchOperation> patch,
            Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
            string dictionaryKey,
            TProperty propertyValue)
        {
            string path = "/" + dictPropSelector.ParseProperty() + "/" + dictionaryKey;

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
            this List<PatchOperation> patch,
            Expression<Func<TEntity, int>> propertySelector,
            int propertyValue)
        {
            string path = "/" + propertySelector.ParseProperty();
            patch.Add(PatchOperation.Increment(path, propertyValue));
        }

        public static void IncrementProperty<TEntity>(
            this List<PatchOperation> patch,
            Expression<Func<TEntity, long>> propertySelector,
            long propertyValue)
        {
            string path = "/" + propertySelector.ParseProperty();
            patch.Add(PatchOperation.Increment(path, propertyValue));
        }
    }

    public sealed class CosmosPatch<TEntity>
    {
        private readonly Container _container;
        private readonly PartitionKey _partitionKey;
        private readonly string _id;
        private readonly List<PatchOperation> _operations;
        private string _concurrencyGuard;
        private bool _ignorePreconditionFailure;

        internal CosmosPatch(Container container, string id, PartitionKey partitionKey)
        {
            _container = container;
            _partitionKey = partitionKey;
            _id = id;
            _operations = new();
        }

        public CosmosPatch<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> propertySelector,
            TProperty propertyValue)
        {
            _operations.SetProperty(propertySelector, propertyValue);
            return this;
        }

        public CosmosPatch<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
            string dictionaryKey,
            TProperty propertyValue)
        {
            _operations.SetProperty(dictPropSelector, dictionaryKey, propertyValue);
            return this;
        }

        public CosmosPatch<TEntity> IncrementProperty(
            Expression<Func<TEntity, int>> propertySelector,
            int propertyValue)
        {
            _operations.IncrementProperty(propertySelector, propertyValue);
            return this;
        }

        public CosmosPatch<TEntity> IncrementProperty(
            Expression<Func<TEntity, long>> propertySelector,
            long propertyValue)
        {
            _operations.IncrementProperty(propertySelector, propertyValue);
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
