using DbManager.Data;

namespace DbManager.Neo4j.Interfaces
{
    public interface IRepository<TNode> where TNode : INode
    {
        /// <summary>
        /// Add node with properties to DB. If node with such id already exist in DB, then node won't added to DB
        /// </summary>
        /// <param name="entity">New node</param>
        /// <returns></returns>
        Task AddNodeAsync(TNode node);

        /// <summary>
        /// Update existing node
        /// </summary>
        /// <param name="node">The node, which will be updated</param>
        /// <returns></returns>
        Task UpdateNodeAsync(TNode node);

        /// <summary>
        /// Get the node with the specified id. If DB return count of nodes != 1, then function throw Exception
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node with specified id</returns>
        /// <exception cref="Exception">Count of items with specified id don't equels 1.</exception>
        Task<TNode> GetNodeAsync(int id);

        /// <summary>
        /// Get all nodes TNode type
        /// </summary>
        /// <returns>List of TNode type</returns>
        Task<List<TNode>> GetNodesAsync();

        /// <summary>
        /// Delete existing node
        /// </summary>
        /// <param name="node">The node, which will be deleted</param>
        /// <returns></returns>
        Task DeleteNodeWithAllRelations(TNode node);

        Task<TRelation> GetRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode;

        Task UpdateRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelation updatedRelation, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode;

        Task DeleteRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode;

        /// <summary>
        /// Get relations and related nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of searched relation</typeparam>
        /// <typeparam name="TRelatedNode">The type of related nodes</typeparam>
        /// <param name="node">node, which have related nodes</param>
        /// <param name="relationInEntity">determines the direction of relation</param>
        /// <returns>If target node don't have related nodes, will be returned empty lists</returns>
        Task<(List<TRelation>, List<TRelatedNode>)> GetRelatedNodesAsync<TRelation, TRelatedNode>(TNode node, bool relationInEntity)
            where TRelation : IRelation
            where TRelatedNode : INode;

        /// <summary>
        /// Relate two existing nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of added relation</typeparam>
        /// <typeparam name="TRelatedNode">the type of node, which will be related</typeparam>
        /// <param name="node">The first node</param>
        /// <param name="relation">The relation, which will be related nodes</param>
        /// <param name="otherNode">The second node</param>
        /// <param name="relationInEntity">determines the direction of relation</param>
        /// <returns></returns>
        Task RelateNodes<TRelation, TRelatedNode>(TNode node, TRelation relation, TRelatedNode otherNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode;
    }
}
