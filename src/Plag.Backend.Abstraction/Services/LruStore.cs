using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class LruStore<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<Entity>> _quickAccess;
        private readonly LinkedList<Entity> _list;

        public LruStore(int capacity = 1000)
        {
            _list = new LinkedList<Entity>();
            _quickAccess = new Dictionary<TKey, LinkedListNode<Entity>>();
            _capacity = capacity;
        }

        public void Clear()
        {
            _quickAccess.Clear();
            _list.Clear();
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (!_quickAccess.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            _list.Remove(node);
            _list.AddFirst(node);
            value = node.Value.Value;
            return true;
        }

        public void Set(TKey key, TValue value)
        {
            if (_quickAccess.ContainsKey(key))
            {
                throw new InvalidOperationException("What the hell?");
            }

            if (_list.Count >= _capacity)
            {
                for (int i = 0; i < _capacity / 20; i++)
                {
                    var toRemove = _list.Last.Value.Key;
                    _quickAccess.Remove(toRemove);
                    _list.RemoveLast();
                }
            }

            var node = _list.AddFirst(new Entity(key, value));
            _quickAccess.Add(key, node);
        }

        /// <remarks>NOT thread safe. Please check <paramref name="key"/> is not null.</remarks>
        public async ValueTask<TValue> GetOrLoadAsync(TKey key, Func<TKey, Task<TValue>> valueFactory)
        {
            if (!TryGet(key, out var value))
            {
                value = await valueFactory(key);
                Set(key, value);
            }

            return value;
        }

        /// <remarks>NOT thread safe. Please check key is not null.</remarks>
        public async ValueTask LoadBatchAsync(List<TKey> keys, Func<List<TKey>, Task<Dictionary<TKey, TValue>>> valueFactory)
        {
            List<TKey> nonexistenceKeys = new();
            foreach (var key in keys)
            {
                if (!TryGet(key, out _))
                {
                    nonexistenceKeys.Add(key);
                }
            }

            if (nonexistenceKeys.Count > 0)
            {
                var batchResults = await valueFactory(nonexistenceKeys);
                foreach (var (key, value) in batchResults)
                {
                    Set(key, value);
                }
            }
        }

        /// <remarks>NOT thread safe. Please check <paramref name="key"/> is not null.</remarks>
        public async ValueTask<TValue> GetOrLoadAsync<TContext>(TKey key, TContext context, Func<TKey, TContext, Task<TValue>> valueFactory)
        {
            if (!TryGet(key, out var value))
            {
                value = await valueFactory(key, context);
                Set(key, value);
            }

            return value;
        }

        private class Entity
        {
            public TValue Value { get; }

            public TKey Key { get; }

            public Entity(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
