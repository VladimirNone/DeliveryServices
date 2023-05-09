using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class ClientRepository : GeneralRepository<Client>, IClientRepository
    {
        public ClientRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public async Task<List<(Client, double, int)>> GetTopClientBySumPriceOrder(int topCount)
        {
            /*match (c:Client)-[r:ORDERED]-(o:Order)
            with c, sum(o.Price) as sum, count(o) as count
            return c,sum,count order by sum limit 10*/

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(Client).Name})-[relation:{typeof(Ordered).Name.ToUpper()}]-(relatedNode:{typeof(Order).Name})")
                .With("node, sum(relatedNode.Price) as sum, count(relatedNode) as count")
                //.Where($"")
                .Return((node, sum, count) => new
                {
                    client = node.As<Client>(),
                    sum = sum.As<double>(),
                    count = count.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "sum DESC" }, limitCount: topCount)
                .ResultsAsync;

            return res.Select(h => (h.client, h.sum, h.count)).ToList();
        }
    }
}
