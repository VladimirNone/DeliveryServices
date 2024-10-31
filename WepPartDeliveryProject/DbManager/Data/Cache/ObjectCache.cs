using System.Collections.Concurrent;

namespace DbManager.Data.Cache
{
    public class ObjectCache<T> : IObjectCache<T> where T : IModel
    {
        private readonly ConcurrentDictionary<Guid, T> _cache = new ConcurrentDictionary<Guid, T> ();

        public event EventHandler<ResolveItemEventArgs<T, Guid>> ResolveItem;

        private static object _accessRoot = new object ();

        private static ObjectCache<T> _instance;

        public static ObjectCache<T> Instanse
        {
            get
            {
                if (_instance == null)
                    lock (_accessRoot)
                        if (_instance == null)
                            _instance = new ObjectCache<T>();
                return _instance;
            }
        }

        public T Get(Guid key)
        {
            if(this._cache.TryGetValue(key, out var value))
                return value;

            if(this.ResolveItem != null)
            {
                var e = new ResolveItemEventArgs<T, Guid>(key);
                this.ResolveItem.Invoke(this, e);
                if(e.ResolvedItem != null)
                {
                    this.Add(key, e.ResolvedItem);
                    return e.ResolvedItem;
                }
            }

            return default;
        }

        public void Add(Guid key, T value)
        {
            if (!this._cache.TryAdd(key, value))
                throw new ArgumentException($"Object with type = {typeof(T)} and key = {key} already exist in cache.");
        }

        public void Update(Guid key, T value)
        {
            if (!this._cache.TryGetValue(key, out var oldValue))
                throw new ArgumentException($"Object with type = {typeof(T)} and key = {key} were not exist in cache and can't be updated.");

            if (!this._cache.TryUpdate(key, value, oldValue))
                throw new ArgumentException($"Object with type = {typeof(T)} and key = {key} were not updated in cache. It were updated other process.");
        }

        public bool TryRemove(Guid key, out T value)
        {
            if(this._cache.TryRemove(key, out value))
                return true;
            return false;
        }
    }
}
