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
    public class DeliveryManRepository : GeneralRepository<DeliveryMan>, IDeliveryManRepository
    {
        public DeliveryManRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public async Task<List<(DeliveryMan, int)>> GetTopDeliveryMenByCountOrder(int topCount)
        {
            /*match (c:DeliveryMan)-[r:DELIVEREDBY]-(o:Order)
            with c, count(o) as count
            return c,count order by COUNT desc limit 10*/

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(DeliveryMan).Name})-[relation:{typeof(DeliveredBy).Name.ToUpper()}]-(relatedNode:{typeof(Order).Name})")
                .With("node, count(relatedNode) as count")
                //.Where($"")
                .Return((node, count) => new
                {
                    delMan = node.As<DeliveryMan>(),
                    count = count.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "count DESC" }, limitCount: topCount)
                .ResultsAsync;

            return res.Select(h => (h.delMan, h.count)).ToList();
        }
    }
}
