using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    /// <summary>
    /// Interface for repository, which work with nodes Order
    /// </summary>
    public interface IOrderRepository : IGeneralRepository<Order>
    {
        Task<List<Order>> GetOrdersByState(string kitchenId, string orderStateId);
        Task<List<Order>> GetOrdersByState(Guid kitchenId, Guid orderStateId);
        Task MoveOrderToNextStage(Order order);
    }
}
