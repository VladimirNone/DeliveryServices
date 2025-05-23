﻿using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class ClientRepository : GeneralNeo4jRepository<Client>, IClientRepository
    {
        public ClientRepository(BoltGraphClientFactory boltGraphClientFactory, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
        }

        public async Task<Order> GetClientOrder(string userId, string orderId)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(GetClientOrder), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            var direction = GetDirection(typeof(Ordered).Name, "relation");
            var typeNodeFrom = typeof(Ordered).BaseType.GenericTypeArguments[0];

            var cypher = _dbContext.Cypher
                .Match($"(node:{typeof(User).Name} {{Id: $userId}}){direction}(relatedNode:{typeof(Order).Name} {{Id: $orderId}})")
                .WithParams(new
                {
                    userId,
                    orderId,
                })
                .Return(relatedNode => relatedNode.As<Order>());

            activity?.SetTag("cypher.query", cypher.Query.QueryText);

            var res = await cypher.ResultsAsync;

            if (res.Count() != 1)
                throw new Exception($"Count of nodes with such Id don't equels 1. Type: {typeof(Order).Name}");

            return res.First();
        }

        public async Task<List<(Client, double, int)>> GetTopClientBySumPriceOrderStatistic(int topCount)
        {
            /*match (c:Client)-[r:ORDERED]-(o:Order)
            with c, sum(o.Price) as sum, count(o) as count
            return c,sum,count order by sum limit 10*/

            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(GetTopClientBySumPriceOrderStatistic), System.Diagnostics.ActivityKind.Client);
            activity?.SetTag("provider", "neo4j");

            var res = await _dbContext.Cypher
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
