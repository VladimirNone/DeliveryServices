using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    public interface IDishRepository : IGeneralRepository<Dish>
    {
        Task<List<Dish>> SearchDishesByNameAndDescription(string searchText, int? skipCount = null, int? limitCount = null, params string[] orderByProperty);
        Task<List<(Dish, int)>> GetTopDishByCountOrderedStatistic(int topCount);
    }
}
