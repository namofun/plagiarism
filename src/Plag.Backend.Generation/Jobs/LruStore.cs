using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
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

        public TValue Get(TKey key)
        {
            if (!_quickAccess.TryGetValue(key, out var node)) return default;
            _list.Remove(node);
            _list.AddFirst(node);
            return node.Value.Value;
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
            var entity = Get(key);
            if (entity != null) return entity;
            var value = await valueFactory(key);
            Set(key, value);
            return value;
        }

        /// <remarks>NOT thread safe. Please check <paramref name="key"/> is not null.</remarks>
        public async ValueTask<TValue> GetOrLoadAsync<TContext>(TKey key, TContext context, Func<TKey, TContext, Task<TValue>> valueFactory)
        {
            var entity = Get(key);
            if (entity != null) return entity;
            var value = await valueFactory(key, context);
            Set(key, value);
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
