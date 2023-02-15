using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralRepository<TEntity> : IRepository<TEntity> where TEntity : IModel
    {
        private readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

        class Friends
        {
            public int Age { get; set; }
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

        public async Task<List<TEntity>> GetNodesAsync()
        {
            var res = await dbContext.Cypher
                .Match($"(entity:{typeof(TEntity).Name})")
                .Return(entity => entity.As<TEntity>())
                .ResultsAsync;

            return res.ToList();
        }

        private async Task Test()
        {
            /*Merge(user: User { Id: 458 })
            on CREATE
            SET user.Name = 'Jim'*/
            var lisa = new Client { Id = 456, Name = "Lisa" };
            var jim = new Client { Id = 457, Name = "Jim" };
            /*                */

            /*match (client1:User {Id:456}), (client2:User {Id:457})
            create(client1) -[:FRIENDS { age: 2}]->(client2)
            return client1, client2*/
            /*                await dbContext.Cypher
                                .Match("(lisa:User {Id:$lId})", "(jim:User {Id:$jId})")
                                .WithParams(new
                                {
                                    lId = lisa.Id,
                                    jId = jim.Id,
                                })
                                .Create("(lisa)-[:FRIENDS]->(jim)")
                                .ExecuteWithoutResultsAsync();*/

            /*                match(client1: User { Id: 12345})-[fr: FRIENDS]->(client2: User { Id: 458})
                            set fr.age = 4
                            return client1, client2*/
            var res = await dbContext.Cypher
            .Match("(lisa:User {Id:$lId})-[fr:FRIENDS]->(jim:User {Id:$jId})")
                                .WithParams(new
                                {
                                    lId = lisa.Id,
                                    jId = jim.Id,
                                    age = 7
                                })
            .Return(fr => new
            {
                User = fr.As<Friends>(),
            })
            .ResultsAsync;
            throw new NotImplementedException();
        }
    }
}
