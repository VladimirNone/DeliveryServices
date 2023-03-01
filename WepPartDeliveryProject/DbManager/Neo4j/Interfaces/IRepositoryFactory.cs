using DbManager.Data;

namespace DbManager.Neo4j.Interfaces
{
    /// <summary>
    /// Factory for IGeneralRepository, it uses for work with neo4j database
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Get repository for work with database nodes
        /// </summary>
        /// <typeparam name="TEntity">Type node from database</typeparam>
        /// <param name="hasCustomRepository">Return custom repository if it register as service</param>
        /// <returns>IGeneralRepository for work with database</returns>
        IGeneralRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : INode;
    }
}
