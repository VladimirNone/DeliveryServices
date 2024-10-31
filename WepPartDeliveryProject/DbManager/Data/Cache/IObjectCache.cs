
namespace DbManager.Data.Cache
{
    public interface IObjectCache<T>
    {
        T Get(Guid key);
        void Add(Guid key, T value);
        void Update(Guid key, T value);
        bool TryRemove(Guid key, out T value);
    }
}
