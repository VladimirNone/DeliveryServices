 using DbManager.Data;

namespace DbManager.Neo4j.Interfaces
{
    /// <summary>
    /// General interface for repository
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public interface IGeneralRepository<TNode> where TNode : INode
    {
        /// <summary>
        /// Add node with properties to DB. Generate new Id. If node with such id already exist in DB, then node won't added to DB
        /// </summary>
        /// <param name="node">New node</param>
        /// <returns></returns>
        Task AddNodeAsync(TNode node);

        /// <summary>
        /// Add nodes with properties to DB. Generate new Id for each nodes. If node with such id already exist in DB, then node won't added to DB
        /// </summary>
        /// <param name="newNodes">List of new nodes</param>
        /// <returns></returns>
        Task AddNodesAsync(List<TNode> newNodes);

        /// <summary>
        /// Update existing node
        /// </summary>
        /// <param name="node">The node, which will be updated</param>
        /// <returns></returns>
        Task UpdateNodeAsync(TNode node);

        /// <summary>
        /// Get the node with the specified id. If DB return count of nodes < 1, then function throw Exception
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node with specified id</returns>
        /// <exception cref="Exception">Count of items with specified id less 1.</exception>
        Task<TNode> GetNodeAsync(string id);

        /// <summary>
        /// Get the node with the specified id. If DB return count of nodes != 1, then function throw Exception
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node with specified id</returns>
        /// <exception cref="Exception">Count of items with specified id don't equels 1.</exception>
        Task<TNode> GetNodeAsync(Guid id);

        /// <summary>
        /// Get all nodes TModel type
        /// </summary>
        /// <param name="skipCount">Count of nodes will skip</param>
        /// <param name="limitCount">Count of nodes will returner after skip</param>
        /// <param name="orderByProperty">Property names by which to sort. ONLY properties of TNode</param>
        /// <returns>List of TModel type</returns>
        Task<List<TNode>> GetNodesAsync(int? skipCount = null, int? limitCount = null, params string[] orderByProperty);

        /// <summary>
        /// Delete existing node
        /// </summary>
        /// <param name="node">The node, which will be deleted</param>
        /// <returns></returns>
        Task DeleteNodeWithAllRelations(TNode node);

        /// <summary>
        /// Get relation between two nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of relation</typeparam>
        /// <typeparam name="TRelatedNode">Type of related nodes</typeparam>
        /// <param name="node">The first node</param>
        /// <param name="relatedNode">The second node</param>
        /// <param name="skipCount">Count of nodes will skip</param>
        /// <param name="limitCount">Count of nodes will returner after skip</param>
        /// <param name="orderByProperty">Property names by which to sort. ONLY properties of TRelation</param>
        /// <returns></returns>
        Task<TRelation> GetRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TRelation : IRelation
            where TRelatedNode : INode;

        /// <summary>
        /// Update realtion of two existing nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of updated relation</typeparam>
        /// <param name="updatedRelation">The relation, which will be used for update data</param>
        /// <returns></returns>
        Task UpdateRelationNodesAsync<TRelation>(TRelation updatedRelation)
            where TRelation : IRelation;

        /// <summary>
        /// Delete relation between two nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of relation</typeparam>
        /// <typeparam name="TRelatedNode">Type of related nodes</typeparam>
        /// <param name="node">The first node</param>
        /// <param name="relatedNode">The second node</param>
        /// <returns></returns>
        Task DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode)
            where TRelation : IRelation
            where TRelatedNode : INode;

        /// <summary>
        /// Get related nodes as List
        /// </summary>
        /// <typeparam name="TRelation">The type of searched relation</typeparam>
        /// <typeparam name="TRelatedNode">The type of related nodes</typeparam>
        /// <param name="node">Node, which have related nodes</param>
        /// <param name="skipCount">Count of nodes will skip</param>
        /// <param name="limitCount">Count of nodes will returner after skip</param>
        /// <param name="orderByProperty">Property names by which to sort. ONLY properties of TRelatedNode</param>
        /// <returns>If target node don't have related nodes, will be returned empty lists</returns>
        Task<List<TRelation>> GetRelatedNodesAsync<TRelation, TRelatedNode>
            (TNode node, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TRelation : IRelation
            where TRelatedNode : INode;
        
        /// <summary>
        /// Relate two existing nodes
        /// </summary>
        /// <typeparam name="TRelation">The type of added relation</typeparam>
        /// <param name="relation">The relation, which will be related nodes</param>
        /// <returns></returns>
        Task RelateNodesAsync<TRelation>(TRelation relation)
            where TRelation : IRelation;

        /// <summary>
        /// Get all nodes, which haven't specified relation
        /// </summary>
        /// <typeparam name="TRelation">The type of relation, which nodes haven't to use</typeparam>
        /// <param name="skipCount">Count of nodes will skip</param>
        /// <param name="limitCount">Count of nodes will returner after skip</param>
        /// <param name="orderByProperty">Property names by which to sort. ONLY properties of TNode</param>
        /// <returns>Nodes, which haven't related using specified relation</returns>
        Task<List<TNode>> GetNodesWithoutRelation<TRelation>(int? skipCount = null, int? limitCount = null, params string[] orderByProperty);

        /// <summary>
        /// Get nodes by property from params
        /// </summary>
        /// <param name="nameOfProperty">Name of property, which will be use for search</param>
        /// <param name="propertyValues">Values of property</param>
        /// <param name="skipCount">Count of nodes will skip</param>
        /// <param name="limitCount">Count of nodes will returner after skip</param>
        /// <param name="orderByProperty">Property names by which to sort. ONLY properties of TNode</param>
        /// <returns></returns>
        Task<List<TNode>> GetNodesByPropertyAsync(string nameOfProperty, string[] propertyValues, int? skipCount = null, int? limitCount = null, params string[] orderByProperty);

        /// <summary>
        /// Set new type to node
        /// </summary>
        /// <typeparam name="TNewNodeType">New node type</typeparam>
        /// <param name="nodeId">Id of node</param>
        /// <returns></returns>
        Task SetNewNodeType<TNewNodeType>(string nodeId)
            where TNewNodeType : INode;

        /// <summary>
        /// Remove new type to node. If the node doesn't implement this type, then nothing will happen.
        /// </summary>
        /// <typeparam name="TNodeType">Node type which will be removed</typeparam>
        /// <param name="nodeId">Id of node</param>
        /// <returns></returns>
        Task RemoveNodeType<TNodeType>(string nodeId)
            where TNodeType : INode;
    }
}
