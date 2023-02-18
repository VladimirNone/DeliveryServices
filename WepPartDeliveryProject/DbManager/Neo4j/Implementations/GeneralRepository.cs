using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralRepository<TNode> : IGeneralRepository<TNode> 
        where TNode : INode
    {
        protected readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

        public async Task AddNodeAsync(TNode newNode)
        {
            newNode.Id = Guid.NewGuid();
            await dbContext.Cypher
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

        public async Task UpdateNodeAsync(TNode node)
        {
            await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name} {{Id: $id}})")
                .Set("newNode = $updatedEntity")
                .WithParams(new
                {
                    id = node.Id,
                    updatedEntity = node
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TNode> GetNodeAsync(Guid id)
        {
            var res = await dbContext.Cypher
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

        public async Task<List<TNode>> GetNodesAsync()
        {
            var res = await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name})")
                .Return(entity => entity.As<TNode>())
                .ResultsAsync;

            return res.ToList();
        }

        public async Task DeleteNodeWithAllRelations(TNode node)
        {
            await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name} {{Id: $id}})-[rOut]->()")
                .Match($"(newNode)<-[rIn]-()")
                .WithParams(new
                {
                    id = node.Id,
                })
                .Delete("rOut, rIn, newNode")
                .ExecuteWithoutResultsAsync();
        }

        public async Task RelateNodes<TRelation>(TRelation relation)
            where TRelation : IRelation
        {
            var relationInEntity = relation.NodeTo.GetType() == typeof(TNode);

            (var node, var otherNode) = (relation.NodeTo, relation.NodeFrom);

            var direction = GetDirection(relation.GetType().Name.ToUpper(), relationInEntity);

            relation.Id = Guid.NewGuid();

            await dbContext.Cypher
                .Match($"(node:{node.GetType().Name} {{Id: $entityId}}), (otherNode:{otherNode.GetType().Name} {{Id: $otherNodeId}})")
                .Create($"(node){direction}(otherNode)")
                .Set("relation=$newRelation")
                .WithParams(new
                {
                    entityId = node.Id,
                    otherNodeId = otherNode.Id,
                    newRelation = relation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task UpdateRelationNodesAsync<TRelation>(TRelation updatedRelation)
            where TRelation : IRelation
        {
            var relationInEntity = updatedRelation.NodeTo.GetType() == typeof(TNode);

            (var node, var otherNode) = (updatedRelation.NodeTo, updatedRelation.NodeFrom);

            var direction = GetDirection(updatedRelation.GetType().Name.ToUpper(), relationInEntity);

            await dbContext.Cypher
                .Match($"(node:{node.GetType().Name} {{Id: $id}}){direction}(relatedNode:{otherNode.GetType().Name} {{Id: $relatedNodeId}})")
                .Set("relation=$updatedRelation")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = otherNode.Id,
                    updatedRelation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TRelation> GetRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection(typeof(TRelation).Name.ToUpper(), relationInEntity);

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                })
                .Return(relation => relation.As<TRelation>())
                .ResultsAsync;

            if (res.Count() != 1)
                throw new Exception($"Nodes don't have relation ({typeof(TNode).Name})-[{typeof(TRelation).Name.ToUpper()})]-({typeof(TRelatedNode).Name})");

            return res.First();
        }

        public async Task<List<TRelation>> GetRelatedNodesAsync<TRelation,TRelatedNode>(TNode node, bool relationInEntity = false) 
            where TRelation: IRelation
            where TRelatedNode: INode
        {
            var direction = GetDirection(typeof(TRelation).Name.ToUpper(), relationInEntity);

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name})")
                .WithParams(new
                {
                    id = node.Id,
                })
                .Return((relation, relatedNode) => new { 
                    nodeRelations = relation.CollectAs<TRelation>(), 
                    relationNodes = relatedNode.CollectAs<TRelatedNode>()
                })
                .ResultsAsync;

            var fRes = res.Single();
            if(fRes.nodeRelations.Count() != fRes.relationNodes.Count())
                throw new Exception($"Count of nodes don't equals count of relation. (Relations){fRes.nodeRelations.Count()}!=(Nodes){fRes.relationNodes.Count()}");

            //А могу ли я быть уверен, что связь - узел идет 1 к 1?
            var relations = fRes.nodeRelations.ToList();
            var relatedNodes = fRes.relationNodes.ToList();

            for (int i = 0; i < relations.Count; i++)
            {
                relations[i].NodeFrom = relationInEntity ? node : relatedNodes[i];
                relations[i].NodeTo = relationInEntity ? node : relatedNodes[i];
            }

            return relations;
        }

        public async Task DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection(typeof(TRelation).Name.ToUpper(), relationInEntity);

            await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .Delete("relation")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                })
                .ExecuteWithoutResultsAsync();
        }

        /// <summary>
        /// Get string with directed relation. Relation has name type of "relation" + relationInstanceName
        /// </summary>
        /// <param name="nameRelation">Name of the relation in DB</param>
        /// <param name="relationInEntity">Relation input in node or output</param>
        /// <param name="relationInstanceName">Name of relation instance</param>
        /// <returns>String with directed relation</returns>
        protected string GetDirection(string nameRelation, bool relationInEntity = false, string relationInstanceName = "relation")
        {
            var direction = $"-[{relationInstanceName}:{nameRelation}]-";
            
            return relationInEntity ? "<" + direction: direction + ">";
        }
    }
}