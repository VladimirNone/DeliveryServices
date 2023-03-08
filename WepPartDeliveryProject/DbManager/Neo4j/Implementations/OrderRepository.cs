using DbManager.Data;
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
    public class OrderRepository : GeneralRepository<Order>, IOrderRepository
    {
        public OrderRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public async Task<List<Order>> GetOrdersByState(string kitchenId, string nameOfState)
        {
            return await GetOrdersByState(Guid.Parse(kitchenId),OrderState.OrderStatesFromDb.Single(h=>h.NameOfState == nameOfState).Id);
        }

        public async Task<List<Order>> GetOrdersByState(string kitchenId, OrderStateEnum orderState)
        {
            return await GetOrdersByState(Guid.Parse(kitchenId), OrderState.OrderStatesFromDb.Single(h => (OrderStateEnum)h.NumberOfStage == orderState).Id);
        }

        public async Task<List<Order>> GetOrdersByState(Guid kitchenId, Guid orderStateId)
        {
            var directionInOrderCB = GetDirection(typeof(CookedBy).Name.ToUpper(), false, "");
            var directionInOrderHOS = GetDirection(typeof(HasOrderState).Name.ToUpper(), false, "");

            var res = await dbContext.Cypher
                .Match(
                    $"(kitchen:{typeof(Kitchen).Name} {{Id: $kitchenId}})" +
                    $"{directionInOrderCB}" +
                    $"(orders:{typeof(Order).Name})" +
                    $"{directionInOrderHOS}" +
                    $"(orderState:{typeof(OrderState).Name} {{Id: $orderStateId}})")
                .WithParams(new
                    {
                        kitchenId,
                        orderStateId,
                    })
                .Return(orders => orders.CollectAs<Order>())
                .ResultsAsync;

            return res.Count() == 0 ? new List<Order>(): res.SingleOrDefault().ToList();
        }


        public async Task MoveOrderToNextStage(string orderId, string comment)
        {
            var order = await GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Cancelled)
                return;

            await DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);
            var newOrderState = new HasOrderState() 
            {
                Comment = comment, 
                NodeFromId = order.Id, 
                NodeToId = OrderState.OrderStatesFromDb.Single(h => h.NumberOfStage == orderState.NumberOfStage * 2).Id, 
                TimeStartState = DateTime.Now 
            };

            order.Story.Add(newOrderState);
            await RelateNodesAsync(newOrderState);
            await UpdateNodeAsync(order);
        }
    }
}
