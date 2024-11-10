using DbManager.Dal;
using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    public interface IDeliveryManRepository : IGeneralRepository<DeliveryMan>
    {
        Task<List<(DeliveryMan, int)>> GetTopDeliveryMenByCountOrderStatistic(int topCount);
    }
}
