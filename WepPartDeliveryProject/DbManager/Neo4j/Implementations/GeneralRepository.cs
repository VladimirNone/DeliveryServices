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
    public class GeneralRepository<TEntity> : IRepository<TEntity> where TEntity : Model
    {
        private readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

        public async Task AddNodeAsync(TEntity entity)
        {
            await dbContext.Cypher
                .Merge($"(entity:{entity.GetType().Name} {{Id: $id}})")
                .OnCreate()
                .Set("entity = $newEntity")
                .WithParams(new
                {
                    id = entity.Id,
                    newEntity = entity
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task UpdateNodeAsync(TEntity entity)
        {
            await dbContext.Cypher
                .Match($"(entity:{entity.GetType().Name} {{Id: $id}})")
                .Set("entity = $updatedEntity")
                .WithParams(new
                {
                    id = entity.Id,
                    updatedEntity = entity
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<TEntity> GetNodeAsync(int id)
        {
            var res = await dbContext.Cypher
                .Match($"(entity:{typeof(TEntity).Name} {{Id: $id}})")
                .WithParams(new
                {
                    id,
                })
                .Return(entity => entity.As<TEntity>())
                .ResultsAsync;
            if (res.Count() != 1)
                throw new Exception("Count of items with Id don't equels 1. Type: " + typeof(TEntity).Name);

            return res.First();
        }

        private TEntity GetEntityFromNeo4jNode(ICypherResultItem result)
        {
            var res = result.As<TEntity>();
            res.Id = result.Id();
            return res;
        }

        public async Task<List<TEntity>> GetNodesAsync()
        {
            var res = await dbContext.Cypher
                .Match($"(entity:{typeof(TEntity).Name})")
                .Return(entity => entity.As<TEntity>())
                .ResultsAsync;

            return res.ToList();
        }

        public async Task DeleteNodeWithRelations(TEntity entity)
        {
            await dbContext.Cypher
                .Match($"(entity:{entity.GetType().Name} {{Id: $id}})-[rOut]->()")
                .Match($"(entity)<-[rIn]-()")
                .WithParams(new
                {
                    id = entity.Id,
                })
                .Delete("rOut, rIn, entity")
                .ExecuteWithoutResultsAsync();
        }
    }
}
