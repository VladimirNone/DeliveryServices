using DbManager.Data;

namespace DbManager.Neo4j.Interfaces
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : INode;
    }
}
