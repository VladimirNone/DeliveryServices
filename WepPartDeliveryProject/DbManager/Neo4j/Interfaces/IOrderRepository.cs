using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
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
        Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(string nodeId, string nameOfState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode;
        Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(string nodeId, OrderStateEnum orderState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode;
        Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(Guid nodeId, Guid orderStateId, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode;
        Task<HasOrderState?> MoveOrderToNextStage(string orderId, string comment);
        Task<bool> MoveOrderToPreviousStage(string orderId);
        Task<List<(string, double, int)>> GetOrderPriceAndCountStatistic();
        Task<List<(string, int, int)>> GetCountFinishedOrderAndClientsStatistic();
        Task<List<(string, List<Order>)>> GetCancelledOrderGroupedByMonthStatistic();
        Task<List<(Kitchen, int, int)>> GetCountOrdersAndOrderedDishesForEveryKitchenStatistic();
    }
}
