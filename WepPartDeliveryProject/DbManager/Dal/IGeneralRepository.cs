using DbManager.Data;

namespace DbManager.Dal
{
    /// <summary>
    /// General interface for repository
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public interface IGeneralRepository : IDisposable
    {
        /// <summary>
        /// Add node with properties to DB. Generate new Id. If node with such id already exist in DB, then node won't added to DB
        /// </summary>
        /// <param name="node">New node</param>
        /// <returns></returns>
        Task AddNodeAsync(INode node);

        /// <summary>
        /// Replace existing node
        /// </summary>
        /// <param name="node">The node, which will be replaced</param>
        /// <returns></returns>
        Task UpdateNodeAsync(INode node);

        /// <summary>
        /// Get the node with the specified id. If DB return count of nodes < 1, then function throw Exception
        /// </summary>
        /// <param name="id">Node id</param>
        /// <param name="typeOfNode">Type of node</param>
        /// <returns>Node with specified id</returns>
        /// <exception cref="Exception">Count of items with specified id less 1.</exception>
        Task<INode> GetNodeAsync(string id, Type typeOfNode);

        /// <summary>
        /// Delete existing node
        /// </summary>
        /// <param name="node">The node, which will be deleted</param>
        /// <returns></returns>
        Task DeleteNodeWithAllRelations(INode node);
    }
}
