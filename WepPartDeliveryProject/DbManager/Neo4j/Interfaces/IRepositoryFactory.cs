using DbManager.Data;

namespace DbManager.Neo4j.Interfaces
{
    public interface IRepositoryFactory
    {
        IGeneralRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : INode;
    }
}
