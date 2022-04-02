using System.Linq.Expressions;
using System.Text;

namespace Xylab.DataAccess.Cosmos
{
    public class Batch<TEntity>
    {
        private readonly TransactionalBatch _batch;
        private readonly PartitionKey _partitionKey;
        private readonly QueryProvider _query;
        private readonly HashSet<string> _queryDesciption;

        internal Batch(TransactionalBatch batch, QueryProvider query, PartitionKey partitionKey)
        {
            _batch = batch;
            _query = query;
            _partitionKey = partitionKey;
            _queryDesciption = new();
        }

        public void CreateItem(TEntity item)
        {
            _batch.CreateItem(item, new() { EnableContentResponseOnWrite = false });
            _queryDesciption.Add("CREATE");
        }

        public void ReadItem(string id)
        {
            _batch.ReadItem(id);
            _queryDesciption.Add("READ FOR @id");
        }

        public void UpsertItem(TEntity item)
        {
            _batch.UpsertItem(item, new() { EnableContentResponseOnWrite = false });
            _queryDesciption.Add("UPSERT");
        }

        public void ReplaceItem(string id, TEntity item)
        {
            _batch.ReplaceItem(id, item, new() { EnableContentResponseOnWrite = false });
            _queryDesciption.Add("REPLACE FOR @id");
        }

        public void DeleteItem(string id)
        {
            _batch.DeleteItem(id);
            _queryDesciption.Add("DELETE FOR @id");
        }

        public PatchBuilder Patch()
        {
            return new PatchBuilder(
                new List<PatchOperation>(),
                new TransactionalBatchPatchItemRequestOptions { EnableContentResponseOnWrite = false },
                this);
        }

        private string CreateDescription(string type)
        {
            StringBuilder sb = new();
            sb.Append(type).Append(" OVER ").Append(_partitionKey);
            foreach (string query in _queryDesciption)
                sb.Append("\r\n").Append(query);
            return sb.ToString();
        }

        public Task<TransactionalBatchResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return _query.Query(_batch, null, true, CreateDescription("TRANSACTIONAL BATCH"), cancellationToken);
        }

        public Task<TransactionalBatchResponse> ExecuteNonTransactionalAsync(CancellationToken cancellationToken = default)
        {
            return _query.Query(_batch, null, false, CreateDescription("NON-TRANSACTIONAL BATCH"), cancellationToken);
        }

        public sealed class PatchBuilder
        {
            private readonly List<PatchOperation> _operations;
            private readonly TransactionalBatchPatchItemRequestOptions _options;
            private readonly Batch<TEntity> _batch;
            private readonly QueryProvider _queryProvider;
            private string? _filterPredicate;

            internal PatchBuilder(
                List<PatchOperation> operations,
                TransactionalBatchPatchItemRequestOptions options,
                Batch<TEntity> batch)
            {
                _operations = operations;
                _options = options;
                _batch = batch;
                _queryProvider = batch._query;
            }

            public PatchBuilder Set<TProperty>(
                Expression<Func<TEntity, TProperty>> propertySelector,
                TProperty propertyValue)
            {
                InternalExtensions.SetProperty(_queryProvider, _operations, propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder Set<TProperty>(
                Expression<Func<TEntity, Dictionary<string, TProperty>>> dictPropSelector,
                string dictionaryKey,
                TProperty propertyValue)
            {
                InternalExtensions.SetProperty(_queryProvider, _operations, dictPropSelector, dictionaryKey, propertyValue);
                return this;
            }

            public PatchBuilder Increment(
                Expression<Func<TEntity, int>> propertySelector,
                int propertyValue)
            {
                InternalExtensions.IncrementProperty(_queryProvider, _operations, propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder Increment(
                Expression<Func<TEntity, long>> propertySelector,
                long propertyValue)
            {
                InternalExtensions.IncrementProperty(_queryProvider, _operations, propertySelector, propertyValue);
                return this;
            }

            public PatchBuilder When(string filterPredicate)
            {
                _filterPredicate = _options.FilterPredicate = filterPredicate;
                return this;
            }

            public PatchBuilder When(string filterPredicate, object param1)
            {
                _filterPredicate = filterPredicate;
                _options.FilterPredicate = filterPredicate.Replace("@param1", param1.ToJson());
                return this;
            }

            public void OnItem(string id)
            {
                _batch._batch.PatchItem(id, _operations, _options);
                _batch._queryDesciption.Add($"PATCH {string.Join(", ", _operations.Select(o => $"{o.OperationType}(\"{o.Path}\")"))}" + (_filterPredicate == null ? "" : " " + _filterPredicate));
            }
        }
    }
}
