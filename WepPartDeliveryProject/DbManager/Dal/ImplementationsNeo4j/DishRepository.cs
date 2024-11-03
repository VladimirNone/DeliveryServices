using DbManager.AppSettings;
using DbManager.Dal.ImplementationsKafka;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Options;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class DishRepository : GeneralKafkaRepository<Dish>, IDishRepository
    {
        public DishRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaDependentProducer<string, string> kafkaProducer, IOptions<KafkaSettings> kafkaOptions) 
            : base(boltGraphClientFactory, kafkaProducer, kafkaOptions)
        {
        }

        public async Task<List<Dish>> SearchDishesByNameAndDescription(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var res = await _dbContext.Cypher
                .Match($"(node:{typeof(Dish).Name})")
                .Where($"toLower(node.Name) contains($searchText) or toLower(node.Description) contains($searchText)")
                .WithParams(new
                    {
                        searchText
                    })
                .Return((node) => node.As<Dish>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            return res.ToList();
        }

        public async Task<List<(Dish, int)>> GetTopDishByCountOrderedStatistic(int topCount)
        {
            /*match (o:Order)-[r:ORDEREDDISH]-(d:Dish) 
            with count(r) as countR, d
            return d, countR order by countR desc limit 10*/

            var res = await _dbContext.Cypher
                .Match($"(node:{typeof(Order).Name})-[relation:{typeof(OrderedDish).Name.ToUpper()}]-(relatedNode:{typeof(Dish).Name})")
                .With("relatedNode, count(relation) as countR")
                //where time > date("2022-11-01") and time < date("2023-03-01")
                .Return((relatedNode, countR) => new
                {
                    dish = relatedNode.As<Dish>(),
                    countOfOrdereds = countR.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "countR desc" }, limitCount: topCount)
                .ResultsAsync;

            return res.Select(h => (h.dish, h.countOfOrdereds)).ToList();
        }
    }
}
