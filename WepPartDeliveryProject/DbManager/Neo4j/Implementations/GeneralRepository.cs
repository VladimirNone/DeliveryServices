using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralRepository<TNode> : IRepository<TNode> where TNode : INode
    {
        private readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

        public async Task AddNodeAsync(TNode newNode)
        {
            //Тут нужно генерить Guid Id узла
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

        public async Task<TNode> GetNodeAsync(int id)
        {
            var res = await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id,
                })
                .Return(entity => entity.As<TNode>())
                .ResultsAsync;
            if (res.Count() != 1)
                throw new Exception("Count of items with Id don't equels 1. Type: " + typeof(TNode).Name);

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

        public async Task RelateNodes<TRelation, TRelatedNode>(TNode node, TRelation relation, TRelatedNode otherNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection<IRelation>(relationInEntity);
            //и тут нужно генерить Guid Id связи
            //и возможно каждый из узлов отправить в addnode, на тот случай, если они не существуют
            //в теории, если у узла значения в Id, то он не существует. Даже на фронт будет отправляться с Id
            await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $entityId}}), (otherNode:{typeof(TRelatedNode).Name} {{Id: $otherNodeId}})")
                .Create($"(node){direction}(otherNode)")
                .Set("updatedRelation=$newRelation")
                .WithParams(new
                {
                    entityId = node.Id,
                    otherNodeId = otherNode.Id,
                    newRelation = relation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task UpdateRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelation updatedRelation, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection<IRelation>(relationInEntity);

            await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .Set("relation=$updatedRelation")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                    updatedRelation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TRelation> GetRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection<IRelation>(relationInEntity);

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name} {{Id: $relatedNodeId}})")
                .WithParams(new
                {
                    id = node.Id,
                    relatedNodeId = relatedNode.Id,
                })
                .Return(relation => relation.As<TRelation>())
                .ResultsAsync;

            return res.First();
        }

        public async Task<(List<TRelation>,List<TRelatedNode>)> GetRelatedNodesAsync<TRelation,TRelatedNode>(TNode node, bool relationInEntity = false) 
            where TRelation: IRelation
            where TRelatedNode: INode
        {
            var direction = GetDirection<IRelation>(relationInEntity);

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

            return (fRes.nodeRelations.ToList<TRelation>(), fRes.relationNodes.ToList<TRelatedNode>());
        }

        public async Task DeleteRelationNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : INode
        {
            var direction = GetDirection<IRelation>();

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
        /// Get string with directed updatedRelation. Relation has name "updatedRelation"
        /// </summary>
        /// <typeparam name="TRelation">Type of updatedRelation. Used name of type for named updatedRelation</typeparam>
        /// <param name="relationInEntity">Relation input in node or output</param>
        /// <returns>String with directed updatedRelation</returns>
        private string GetDirection<TRelation>(bool relationInEntity = false) where TRelation : IRelation
        {
            var direction = $"-[relation:{typeof(TRelation).Name.ToUpper()}]-";

            return relationInEntity ? "<" + direction: direction + ">";
        }
    }
}
