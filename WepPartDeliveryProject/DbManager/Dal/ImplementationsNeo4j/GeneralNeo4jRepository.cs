using DbManager.Dal;
using DbManager.Data;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralNeo4jRepository<TNode> : IGeneralRepository<TNode>
        where TNode : INode
    {
        protected readonly IGraphClient _dbContext;

        public GeneralNeo4jRepository(BoltGraphClientFactory boltGraphClientFactory)
        {
            this._dbContext = boltGraphClientFactory.GetGraphClient();
        }

        public virtual async Task AddNodeAsync(TNode newNode)
        {
            if(newNode.Id == Guid.Empty)
                newNode.Id = Guid.NewGuid();
            await _dbContext.Cypher
                .Merge($"(newNode:{typeof(TNode).Name} {{Id: $id}})")
                .OnCreate()
                .Set("newNode = $newEntity")
                .WithParams(new
                {
                    id = newNode.Id,
                    newEntity = newNode
                })  
                .ExecuteWithoutResultsAsync();
        }

        public async Task AddNodesAsync(List<TNode> newNodes)
        {
            foreach (var item in newNodes)
            {
                await AddNodeAsync(item);
            }
        }

        public virtual async Task UpdateNodeAsync(TNode node)
        {
            await _dbContext.Cypher
                .Match($"(updateNode:{typeof(TNode).Name} {{Id: $id}})")
                .Set("updateNode = $updatedEntity")
                .WithParams(new
                {
                    id = node.Id,
                    updatedEntity = node
                })
                .ExecuteWithoutResultsAsync();
        }

        public virtual async Task UpdateNodesPropertiesAsync(TNode node)
        {
            var properties = typeof(TNode).GetProperties();
            var query = _dbContext.Cypher
                .Match($"(updateNode:{typeof(TNode).Name} {{Id: $id}})");

            foreach (var property in properties)
            {
                query = query.Set($"updateNode.{property.Name} = $updatedEntity.{property.Name}");
            }

            await query.WithParams(new
            {
                id = node.Id,
                updatedEntity = node
            })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TNode> GetNodeAsync(string id)
            => await GetNodeAsync(Guid.Parse(id));

        public async Task<TNode> GetNodeAsync(Guid id)
        {
            var res = await _dbContext.Cypher
                .Match($"(entity:{typeof(TNode).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id,
                })
                .Return(entity => entity.As<TNode>())
                .ResultsAsync;

            if (res.Count() != 1)
                throw new Exception($"Count of nodes with such Id don't equels 1. Type: {typeof(TNode).Name}");

            return res.First();
        }

        public async Task<List<TNode>> GetNodesAsync(int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "entity." + orderByProperty[i];

            var query = _dbContext.Cypher
                .Match($"(entity:{typeof(TNode).Name})")
                .Return(entity => entity.As<TNode>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount);

            var res = await query.ResultsAsync;

            return res.ToList();
        }

        public virtual async Task DeleteNodeWithAllRelations(TNode node)
        {
            await _dbContext.Cypher
                .Match($"(entity:{typeof(TNode).Name} {{Id: $id}})-[r]-()")
                .WithParams(new
                {
                    id = node.Id,
                })
                .Delete("r, entity")
                .ExecuteWithoutResultsAsync();
        }

        public virtual async Task RelateNodesAsync<TRelation>(TRelation relation)
            where TRelation : IRelation
        {
            if(relation.NodeFromId == null || relation.NodeToId == null)
            { 
                throw new Exception("NodeFromId or NodeToId is null. Method RelateNodesAsync where TRelation is " + typeof(TRelation));
            }
            var typeNodeFrom = typeof(TRelation).BaseType.GenericTypeArguments[0];
            var direction = GetDirection(relation.GetType().Name, "relation", typeNodeFrom == typeof(TNode));

            if (relation.Id == Guid.Empty)
                relation.Id = Guid.NewGuid();

            await _dbContext.Cypher
                .Match($"(node {{Id: $entityId}}), (otherNode {{Id: $otherNodeId}})")
                .Create($"(node){direction}(otherNode)")
                .Set("relation=$newRelation")
                .WithParams(new
                {
                    entityId = relation.NodeFromId,
                    otherNodeId = relation.NodeToId,
                    newRelation = relation
                })
                .ExecuteWithoutResultsAsync();
        }

        public virtual async Task UpdateRelationNodesAsync<TRelation>(TRelation updatedRelation)
            where TRelation : IRelation
        {
            var typeNodeFrom = typeof(TRelation).BaseType.GenericTypeArguments[0];
            var direction = GetDirection(updatedRelation.GetType().Name, "relation", typeNodeFrom == typeof(TNode));

            await _dbContext.Cypher
                .Match($"(node {{Id: $id}}){direction}(relatedNode {{Id: $relatedNodeId}})")
                .Set("relation=$updatedRelation")
                .WithParams(new
                {
                    id = updatedRelation.NodeFromId,
                    relatedNodeId = updatedRelation.NodeToId,
                    updatedRelation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TRelation> GetRelationBetweenTwoNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "relation." + orderByProperty[i];

            var direction = GetDirection(typeof(TRelation).Name, "relation");

            var res = await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                })
                .Return(relation => relation.As<TRelation>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            if (res.Count() < 1)
                throw new Exception($"Nodes don't have relation ({typeof(TNode).Name})-[{typeof(TRelation).Name.ToUpper()})]-({typeof(TRelatedNode).Name})");

            return res.First();
        }

        public async Task<List<TRelation>> GetRelationsOfNodesAsync<TRelation, TRelatedNode>(TNode node, int? skipCount = null, int? limitCount = null, params string[] orderByProperty) 
            where TRelation: IRelation
            where TRelatedNode: INode
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "relatedNode." + orderByProperty[i];

            var direction = GetDirection(typeof(TRelation).Name, "relation");
            var typeNodeFrom = typeof(TRelation).BaseType.GenericTypeArguments[0];

            var res = await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name})")
                .WithParams(new
                {
                    id = node.Id,
                })
                .Return((relation, relatedNode) => new
                {
                    nodeRelations = relation.As<TRelation>(),
                    relationNodes = relatedNode.As<TRelatedNode>()
                })
                .ChangeQueryForPaginationAnonymousType(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            var nodeIsNodeFrom = typeof(TNode) == typeNodeFrom;
            var relations = res.Select(h =>
            {
                h.nodeRelations.NodeFrom = nodeIsNodeFrom ? node : h.relationNodes;
                h.nodeRelations.NodeTo = nodeIsNodeFrom ? h.relationNodes : node;
                return h.nodeRelations;
            }).ToList();

            return relations;
        }

        public async Task<List<TRelation>> GetRelationsOfNodesAsync<TRelation, TRelatedNode>(string nodeId, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TRelation : IRelation
            where TRelatedNode : INode 
        {
            var node = await GetNodeAsync(nodeId);

            return await GetRelationsOfNodesAsync<TRelation, TRelatedNode>(node, skipCount, limitCount, orderByProperty);
        }

        public virtual async Task DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection(typeof(TRelation).Name, "relation");

            await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .Delete("relation")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<List<TNode>> GetNodesWithoutRelation<TRelation>(int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var directionIn = GetDirection(typeof(TRelation).Name); 
            var directionOut = GetDirection(typeof(TRelation).Name);

            var result = await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name})")
                .Where($"not (node){directionIn}() and not (node){directionOut}()")
                .Return(node => node.As<TNode>())
                .ChangeQueryForPagination(orderByProperty,skipCount,limitCount)
                .ResultsAsync;

            return result.ToList();
        }

        public async Task<List<TNode>> GetNodesByIdAsync(string[] ids, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var result = await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name})")
                .Where($"node.Id in [\"{string.Join("\",\"", ids)}\"]")
                .Return(node => node.As<TNode>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            return result.ToList();
        }

        public async Task<List<TNode>> GetNodesByPropertyAsync(string nameOfProperty, string[] propertyValues, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var result = await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name})")
                .Where($"node.{nameOfProperty} in [\"{string.Join("\",\"", propertyValues)}\"]")
                .Return(node => node.As<TNode>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            return result.ToList();
        }

        public virtual async Task SetNewNodeType<TNewNodeType>(string nodeId)
            where TNewNodeType : INode
        {
            await SetNewNodeType(nodeId, typeof(TNewNodeType).Name);
        }

        public virtual async Task SetNewNodeType(string nodeId, string nodeTypeName)
        {
            await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}})")
                .Set($"node:{nodeTypeName}")
                .WithParams(new
                {
                    id = nodeId,
                })
                .ExecuteWithoutResultsAsync();
        }

        public virtual async Task RemoveNodeType<TNodeType>(string nodeId)
            where TNodeType : INode
        {
            await RemoveNodeType(nodeId, typeof(TNodeType).Name);
        }

        public virtual async Task RemoveNodeType(string nodeId, string nodeTypeName)
        {
            await _dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}})")
                .Remove($"node:{nodeTypeName}")
                .WithParams(new
                {
                    id = nodeId,
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<bool> HasNodeType<TNodeType>(string nodeId)
            where TNodeType : INode
        {
            return await HasNodeType(nodeId, typeof(TNodeType).Name);
        }

        public async Task<bool> HasNodeType(string nodeId, string nodeType)
        {
            var result = await _dbContext.Cypher
                .Match($"(node:{typeof(User).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id = nodeId,
                })
                .ReturnDistinct<List<string>>("labels(node)")
                .ResultsAsync;

            var clearResult = result.First().ToList();

            if (clearResult.Contains(nodeType))
                return true;

            return false;
        }

        /// <summary>
        /// Get string with directed relation. Relation has name type of "relation" + relationInstanceName
        /// </summary>
        /// <param name="nameRelation">Name of the relation in DB</param>
        /// <param name="relationInstanceName">Name of relation instance</param>
        /// <param name="relationInEntity">Relation input in node or output</param>
        /// <returns>String with directed relation</returns>
        protected string GetDirection(string nameRelation, string? relationInstanceName = "", bool? relationInEntity = null)
        {
            string direction = "-[]-";
            if(!string.IsNullOrEmpty(nameRelation))
                direction = $"-[{relationInstanceName}:{nameRelation.ToUpper()}]-";

            if (relationInEntity == null)
                return direction;

            return relationInEntity.Value ? "<" + direction: direction + ">";
        }

        public void Dispose()
        {
            this._dbContext.Dispose();
        }
    }
}