
namespace DbManager.Data.Cache
{
    public interface IObjectCache<T> where T : IModel
    {
        T Get(Guid key);
        void AddList(List<T> data);
        void Add(Guid key, T value);
        void Update(Guid key, T value);
        bool TryRemove(Guid key, out T value);
    }
}
