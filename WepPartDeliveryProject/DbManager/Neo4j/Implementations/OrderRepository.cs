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

        public async Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(string nodeId, string nameOfState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode
        {
            return await GetOrdersByStateRelatedWithNode<TNode>(Guid.Parse(nodeId),OrderState.OrderStatesFromDb.Single(h=>h.NameOfState == nameOfState).Id, skipCount, limitCount, orderByProperty);
        }

        public async Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(string nodeId, OrderStateEnum orderState, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode
        {
            return await GetOrdersByStateRelatedWithNode<TNode>(Guid.Parse(nodeId), OrderState.OrderStatesFromDb.Single(h => (OrderStateEnum)h.NumberOfStage == orderState).Id, skipCount, limitCount, orderByProperty);
        }

        public async Task<List<Order>> GetOrdersByStateRelatedWithNode<TNode>(Guid nodeId, Guid orderStateId, int? skipCount = null, int? limitCount = null, params string[] orderByProperty)
            where TNode : INode
        {
            for (int i = 0; i < orderByProperty.Length; i++)
                orderByProperty[i] = "orders." + orderByProperty[i];

            var directionInOrderCB = GetDirection("");
            var directionInOrderHOS = GetDirection(typeof(HasOrderState).Name, "rel");

            var res = await dbContext.Cypher
                .Match(
                    $"(node:{typeof(TNode).Name} {{Id: $nodeId}})" +
                    $"{directionInOrderCB}" +
                    $"(orders:{typeof(Order).Name})" +
                    $"{directionInOrderHOS}" +
                    $"(orderState:{typeof(OrderState).Name} {{Id: $orderStateId}})")
                .WithParams(new
                    {
                        nodeId,
                        orderStateId,
                    })
                .Return(orders => orders.As<Order>())
                .ChangeQueryForPagination(orderByProperty.Concat(new[] { "rel.TimeStartState desc" }).ToArray(), skipCount, limitCount)
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

        public async Task<bool> MoveOrderToPreviousStage(string orderId)
        {
            var order = await GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ только попал в очередь
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.InQueue)
                return false;
            
            await DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);
            order.Story.Remove(orderHasState);

            var newOrderState = order.Story.Last();

            await RelateNodesAsync(newOrderState);
            await UpdateNodeAsync(order);

            return true;
        }

        public async Task CreateOrderRelationInDB(Order order, string? userId, List<Dish> dishes, Kitchen kitchen, DeliveryMan deliveryMan, Dictionary<string, int> countOfDishes, string? comment)
        {
            if(userId != null)
                await RelateNodesAsync(new Ordered() { NodeFromId = Guid.Parse(userId), NodeTo = order });

            foreach (var dish in dishes)
            {
                await RelateNodesAsync(new OrderedDish() { NodeFrom = order, NodeTo = dish, Count = countOfDishes[dish.Id.ToString()] });
            }

            await RelateNodesAsync(new CookedBy() { NodeFrom = kitchen, NodeTo = order });

            await RelateNodesAsync(new DeliveredBy() { NodeFrom = deliveryMan, NodeTo = order });

            var firstState = OrderState.OrderStatesFromDb.First(h => h.NumberOfStage == (int)OrderStateEnum.InQueue);
            var relationCancel = new HasOrderState() { Comment = comment, NodeFromId = order.Id, NodeToId = firstState.Id, TimeStartState = DateTime.Now };

            order.Story.Add(relationCancel);
            await RelateNodesAsync(relationCancel);
            await UpdateNodeAsync(order);
        }

        public async Task<List<(string, double, int)>> GetOrderPriceAndCountStatistic()
        {
            /*MATCH (node:Order)-[relation:HASORDERSTATE]-(relatedNode:OrderState {NumberOfStage: 16}) 
            WITH date.truncate('month', datetime(relation.TimeStartState)) as time , sum(node.Price) as sum, count(relatedNode) as count 
            where time > date("2022-11-01") and time < date("2023-03-01")
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

        public async Task<List<(string, int, int)>> GetCountFinishedOrderAndClientsStatistic()
        {
            /*MATCH (node:Client)-[relation:ORDERED]-(relatedNode:Order)-[relation2:HASORDERSTATE]-(relatedNode2:OrderState {NumberOfStage: 16})
            with count(relatedNode) as countOfOrders, count(distinct node) as countOfClients, date.truncate('month', datetime(relation2.TimeStartState)) as time
            RETURN countOfOrders, countOfClients, time order by time */

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(Client).Name})-[relation:{typeof(Ordered).Name.ToUpper()}]-(relatedNode:{typeof(Order).Name})-[relation2:{typeof(HasOrderState).Name.ToUpper()}]-(relatedNode2:{typeof(OrderState).Name} {{NumberOfStage: $NumberOfFinishStage}})")
                .WithParams(new
                {
                    NumberOfFinishStage = (int)OrderStateEnum.Finished,
                })
                .With("count(relatedNode) as countOfOrders, count(distinct node) as countOfClients, date.truncate('month', datetime(relation2.TimeStartState)) as time")
                //where time > date("2022-11-01") and time < date("2023-03-01")
                .Return((time, countOfOrders, countOfClients) => new
                {
                    time = time.As<DateTime>(),
                    countOfOrders = countOfOrders.As<int>(),
                    countOfClients = countOfClients.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "time" })
                .ResultsAsync;

            return res.Select(h => (h.time.Month + "." + h.time.Year, h.countOfOrders, h.countOfClients)).ToList();
        }

        public async Task<List<(string, List<Order>)>> GetCancelledOrderGroupedByMonthStatistic()
        {
            /* MATCH(node:Order)-[relation: HASORDERSTATE]-(relatedNode:OrderState {NumberOfStage: 32})
            with node, date.truncate('month', datetime(relation.TimeStartState)) as time
            RETURN node, time order by time */

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(Order).Name})-[relation:{typeof(HasOrderState).Name.ToUpper()}]-(relatedNode:{typeof(OrderState).Name} {{NumberOfStage: $NumberOfFinishStage}})")
                .WithParams(new
                {
                    NumberOfFinishStage = (int)OrderStateEnum.Cancelled,
                })
                .With("node, date.truncate('month', datetime(relation.TimeStartState)) as time")
                //where time > date("2022-11-01") and time < date("2023-03-01")
                .Return((node, time) => new
                {
                    time = time.As<DateTime>(),
                    node = node.CollectAs<Order>(),
                })
                .ChangeQueryForPaginationAnonymousType(new[] { "time" })
                .ResultsAsync;

            return res.Select(h => (h.time.Month + "." + h.time.Year, h.node.ToList())).ToList();
        }

        public async Task<List<(Kitchen, int, int)>> GetCountOrdersAndOrderedDishesForEveryKitchenStatistic()
        {
            /*match(k:Kitchen)-[]-(o:Order)-[r:ORDEREDDISH]-(d:Dish)
            with k, count(distinct o) as countO, sum(r.Count) as sumD
            return k,countO, sumD */

            var res = await dbContext.Cypher
                .Match($"(node:{typeof(Kitchen).Name})-[relation:{typeof(CookedBy).Name.ToUpper()}]-(relatedNode:{typeof(Order).Name})-[relation2:{typeof(OrderedDish).Name.ToUpper()}]-(relatedNode2:{typeof(Dish).Name})")
                .With("node, count(distinct relatedNode) as countO, sum(relation2.Count) as sumD")
                //where time > date("2022-11-01") and time < date("2023-03-01")
                .Return((node, countO, sumD) => new
                {
                    kitchen = node.As<Kitchen>(),
                    countOfOrders = countO.As<int>(),
                    sumOfDishes = sumD.As<int>(),
                })
                .ChangeQueryForPaginationAnonymousType(new string[0])
                .ResultsAsync;

            return res.Select(h => (h.kitchen, h.countOfOrders, h.sumOfDishes)).ToList();
        }
    }
}
