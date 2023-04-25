using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class DishRepository : GeneralRepository<Dish>, IDishRepository
    {
        public DishRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public async Task<List<Dish>> SearchDishesByNameAndDescription(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "node." + orderByProperty[i];

            var res = await dbContext.Cypher
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
    }
}
