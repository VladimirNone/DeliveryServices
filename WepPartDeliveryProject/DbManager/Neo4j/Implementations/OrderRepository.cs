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

        public async Task<List<Order>> GetOrdersByState(string kitchenId, string nameOfState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            return await GetOrdersByState(Guid.Parse(kitchenId),OrderState.OrderStatesFromDb.Single(h=>h.NameOfState == nameOfState).Id, skipCount, limitCount, orderByProperty);
        }

        public async Task<List<Order>> GetOrdersByState(string kitchenId, OrderStateEnum orderState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            return await GetOrdersByState(Guid.Parse(kitchenId), OrderState.OrderStatesFromDb.Single(h => (OrderStateEnum)h.NumberOfStage == orderState).Id, skipCount, limitCount, orderByProperty);
        }

        public async Task<List<Order>> GetOrdersByState(Guid kitchenId, Guid orderStateId, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "orders." + orderByProperty[i];

            var directionInOrderCB = GetDirection(typeof(CookedBy).Name);
            var directionInOrderHOS = GetDirection(typeof(HasOrderState).Name);

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
                .Return(orders => orders.As<Order>())
                .ChangeQueryForPagination(orderByProperty, skipCount, limitCount)
                .ResultsAsync;

            return res.ToList();
        }

        public async Task<HasOrderState?> MoveOrderToNextStage(string orderId, string comment)
        {
            var order = await GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ был отменен или завершен, то ничего не произойдет
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Cancelled || (OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Finished)
                return null;

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

            newOrderState.NodeTo = OrderState.OrderStatesFromDb.Single(h => h.NumberOfStage == orderState.NumberOfStage * 2);

            return newOrderState;
        }

        public async Task MoveOrderToPreviousStage(string orderId)
        {
            var order = await GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ только попал в очередь
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.InQueue)
                return;
            
            await DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);
            order.Story.Remove(orderHasState);

            var newOrderState = order.Story.Last();

            await RelateNodesAsync(newOrderState);
            await UpdateNodeAsync(order);
        }

        public async Task<List<(string, double, int)>> GetOrderPriceAndCountStatistic()
        {
            /*MATCH (node:Order)-[relation:HASORDERSTATE]-(relatedNode:OrderState {NumberOfStage: 16}) 
            WITH date.truncate('month', datetime(relation.TimeStartState)) as time , sum(node.Price) as sum, count(relatedNode) as count 
            RETURN time, sum, count order by time*/

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(Order).Name})-[relation:{typeof(HasOrderState).Name.ToUpper()}]-(relatedNode:{typeof(OrderState).Name} {{NumberOfStage: $NumberOfFinishStage}})")
                .WithParams(new
                {
                    NumberOfFinishStage = (int)OrderStateEnum.Finished,
                })
                .With("date.truncate('month', datetime(relation.TimeStartState)) as time , sum(node.Price) as sum, count(relatedNode) as count")
                //where time > date("2022-11-01") and time < date("2023-03-01")
                .Return((time, sum, count) => new
                {
                    time = time.As<DateTime>(),
                    sum = sum.As<double>(),
                    count = count.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "time" })
                .ResultsAsync;

            return res.Select(h => (h.time.Month+"."+h.time.Year, h.sum, h.count)).ToList();
        }
    }
}
