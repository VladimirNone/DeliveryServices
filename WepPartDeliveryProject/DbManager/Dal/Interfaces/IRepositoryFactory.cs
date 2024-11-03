using DbManager.Dal;
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
        /// <returns>IGeneralRepository for work with database</returns>
        IGeneralRepository<TEntity> GetRepository<TEntity>() where TEntity : INode;

        /// <summary>
        /// Get repository for work with database nodes
        /// </summary>
        /// <param name="typeOfNode">Type node from database</param>
        /// <returns>IGeneralRepository for work with database</returns>
        IGeneralRepository GetRepository(Type typeOfNode);
    }
}
