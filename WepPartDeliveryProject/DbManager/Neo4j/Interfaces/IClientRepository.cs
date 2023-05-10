using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    public interface IClientRepository : IGeneralRepository<Client>
    {
        public Task<List<(Client, double, int)>> GetTopClientBySumPriceOrderStatistic(int topCount);
    }
}
