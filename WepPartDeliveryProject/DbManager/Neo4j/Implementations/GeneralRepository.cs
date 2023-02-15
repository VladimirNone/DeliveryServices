using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralRepository<TNode> : IRepository<TNode> where TNode : Model
    {
        private readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

        public async Task AddNodeAsync(TNode newNode)
        {
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

        public async Task DeleteNodeWithRelations(TNode node)
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

        public async Task RelateExistingNodes<TRelation, TRelatedNode>(TNode node, TRelation relation, TRelatedNode otherNode, bool relationInEntity = false)
            where TRelation : IRelation
            where TRelatedNode : Model, Data.INode
        {
            var direction = $"-[relation:{typeof(TRelation).Name.ToUpper()}]->";
            if (relationInEntity)
                direction = $"<-[relation:{typeof(TRelation).Name.ToUpper()}]-";

            await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name} {{Id: $entityId}}), (otherNode:{typeof(TRelatedNode).Name} {{Id: $otherNodeId}})")
                .Create($"(newNode){direction}(otherNode)")
                .Set("relation=$newRelation")
                .WithParams(new
                {
                    entityId = node.Id,
                    otherNodeId = otherNode.Id,
                    newRelation = relation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<(List<TRelation>,List<TRelatedNode>)> GetRelatedNodesAsync<TRelation,TRelatedNode>(TNode node, bool relationInEntity = false) 
            where TRelation: IRelation
            where TRelatedNode: Model, Data.INode
        {
            var direction = $"-[relations:{typeof(TRelation).Name.ToUpper()}]->";
            if (relationInEntity)
                direction = $"<-[relations:{typeof(TRelation).Name.ToUpper()}]-";

            var res = await dbContext.Cypher
                .Match($"(newNode:{typeof(TNode).Name} {{Id: $id}}){direction}(relatedNode:{typeof(TRelatedNode).Name})")
                .WithParams(new
                {
                    id = node.Id,
                })
                .Return((relations, relatedNode) => new { 
                    nodeRelations = relations.CollectAs<TRelation>(), 
                    relationNodes = relatedNode.CollectAs<TRelatedNode>() 
                })
                .ResultsAsync;

            var fRes = res.Single();

            return (fRes.nodeRelations.ToList<TRelation>(), fRes.relationNodes.ToList<TRelatedNode>());
        }
    }
}
