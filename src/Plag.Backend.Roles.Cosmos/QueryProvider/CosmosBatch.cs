using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    public class CosmosBatch<TEntity>
    {
        private readonly TransactionalBatch _batch;

        public CosmosBatch(TransactionalBatch batch)
        {
            _batch = batch;
        }

        public void CreateItem(TEntity item)
        {
            _batch.CreateItem(item, new() { EnableContentResponseOnWrite = false });
        }

        public void ReadItem(string id)
        {
            _batch.ReadItem(id);
        }

        public void UpsertItem(TEntity item)
        {
            _batch.UpsertItem(item, new() { EnableContentResponseOnWrite = false });
        }

        public void ReplaceItem(string id, TEntity item)
        {
            _batch.ReplaceItem(id, item, new() { EnableContentResponseOnWrite = false });
        }

        public void DeleteItem(string id)
        {
            _batch.DeleteItem(id);
        }

        public PatchBuilder Patch()
        {
            return new PatchBuilder(
                new List<PatchOperation>(),
                new TransactionalBatchPatchItemRequestOptions { EnableContentResponseOnWrite = false },
                this);
        }

        public Task<TransactionalBatchResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return _batch.ExecuteAsync(cancellationToken);
        }

        public Task<TransactionalBatchResponse> ExecuteNonTransactionalAsync(CancellationToken cancellationToken = default)
        {
            return _batch.ExecuteNonTransactionalAsync(cancellationToken);
        }

        public sealed class PatchBuilder
        {
            private readonly List<PatchOperation> _operations;
            private readonly TransactionalBatchPatchItemRequestOptions _options;
            private readonly CosmosBatch<TEntity> _batch;

            public PatchBuilder(
                List<PatchOperation> operations,
                TransactionalBatchPatchItemRequestOptions options,
                CosmosBatch<TEntity> batch)
            {
                _operations = operations;
                _options = options;
                _batch = batch;
            }

            public PatchBuilder Set<TProperty>(
                Expression<Func<TEntity, TProperty>> propertySelector,
                TProperty propertyValue)
            {
                _operations.SetProperty(propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder Set<TProperty>(
                Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
                string dictionaryKey,
                TProperty propertyValue)
            {
                _operations.SetProperty(dictPropSelector, dictionaryKey, propertyValue);
                return this;
            }

            public PatchBuilder Increment(
                Expression<Func<TEntity, int>> propertySelector,
                int propertyValue)
            {
                _operations.IncrementProperty(propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder Increment(
                Expression<Func<TEntity, long>> propertySelector,
                long propertyValue)
            {
                _operations.IncrementProperty(propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder When(string filterPredicate)
            {
                _options.FilterPredicate = filterPredicate;
                return this;
            }

            public void OnItem(string id)
            {
                _batch._batch.PatchItem(id, _operations, _options);
            }
        }
    }
}
