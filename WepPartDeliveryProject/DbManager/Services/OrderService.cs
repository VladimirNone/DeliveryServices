using DbManager.Data.Cache;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;

namespace DbManager.Services
{
    public class OrderService : IOrderService
    {
        private Instrumentation _instrumentation;
        private readonly IRepositoryFactory _repositoryFactory;

        public OrderService(IRepositoryFactory repositoryFactory, Instrumentation instrumentation)
        {
            this._repositoryFactory = repositoryFactory;
            this._instrumentation = instrumentation;
        }

        public async Task<bool> MoveOrderToNextStage(string orderId, string comment)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(MoveOrderToNextStage), System.Diagnostics.ActivityKind.Client);
            var orderRepo = this._repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = ObjectCache<OrderState>.Instance.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ был отменен или завершен, то ничего не произойдет
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Cancelled || (OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Finished)
                return false;

            await orderRepo.DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);
            var newOrderState = new HasOrderState()
            {
                Comment = comment,
                NodeFromId = order.Id,
                NodeToId = ObjectCache<OrderState>.Instance.Single(h => h.NumberOfStage == orderState.NumberOfStage * 2).Id,
                TimeStartState = DateTime.Now
            };

            order.Story.Add(newOrderState);
            await orderRepo.RelateNodesAsync(newOrderState);
            await orderRepo.UpdateNodeAsync(order);

            newOrderState.NodeTo = ObjectCache<OrderState>.Instance.Single(h => h.NumberOfStage == orderState.NumberOfStage * 2);

            return true;
        }

        public async Task<bool> MoveOrderToPreviousStage(string orderId)
        {
            using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(MoveOrderToPreviousStage), System.Diagnostics.ActivityKind.Client);
            var orderRepo = this._repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = ObjectCache<OrderState>.Instance.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ только попал в очередь
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.InQueue)
                return false;

            await orderRepo.DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);
            order.Story.Remove(orderHasState);

            var newOrderState = order.Story.Last();

            await orderRepo.RelateNodesAsync(newOrderState);
            await orderRepo.UpdateNodeAsync(order);

            return true;
        }

        public async Task CancelOrderedDish(string orderId, string dishId)
        {
            var order = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(orderId);

            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(dishId);

            var orderedDish = await _repositoryFactory.GetRepository<Dish>().GetRelationBetweenTwoNodesAsync<OrderedDish, Order>(dish, order);

            order.Price -= orderedDish.Count * dish.Price;

            await _repositoryFactory.GetRepository<Order>().UpdateNodeAsync(order);

            await _repositoryFactory.GetRepository<Order>().DeleteRelationOfNodesAsync<OrderedDish, Dish>(order, dish);
        }

        public async Task CancelOrder(string orderId, string reasonOfCancel)
        {
            var orderRepo = _repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(orderId);
            var orderHasState = order.Story.Last();
            var orderState = ObjectCache<OrderState>.Instance.Single(h => h.Id == orderHasState.NodeToId);

            await orderRepo.DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);

            var cancelState = ObjectCache<OrderState>.Instance.First(h => h.NumberOfStage == (int)OrderStateEnum.Cancelled);
            var relationCancel = new HasOrderState() { Comment = reasonOfCancel, NodeFromId = order.Id, NodeToId = cancelState.Id, TimeStartState = DateTime.Now };

            order.Story.Add(relationCancel);
            await orderRepo.RelateNodesAsync(relationCancel);
            await orderRepo.UpdateNodeAsync(order);
        }

        public async Task ChangeCountOrderedDish(string orderId, string dishId, int count)
        {
            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>();

            var order = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(orderId);

            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(new Guid(dishId));

            var orderedDish = await _repositoryFactory.GetRepository<Dish>().GetRelationBetweenTwoNodesAsync<OrderedDish, Order>(dish, order);

            order.Price -= orderedDish.Count * dish.Price;

            orderedDish.Count = count;

            order.Price += orderedDish.Count * dish.Price;

            await _repositoryFactory.GetRepository<Order>().UpdateNodeAsync(order);

            await _repositoryFactory.GetRepository<Order>().UpdateRelationNodesAsync(orderedDish);
        }

        public async Task PlaceAnOrder(string orderId, string userId, Dictionary<string, int> dishesCounts, string comment, string phoneNumber, string deliveryAddress)
        {
            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", dishesCounts.Keys.ToArray());

            var order = new Order()
            {
                Id = Guid.Parse(orderId),
                SumWeight = dishes.Sum(h => h.Weight),
                Price = dishes.Sum(h => h.Price),
                DeliveryAddress = deliveryAddress,
                PhoneNumber = phoneNumber
            };

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>();
            await orderRepo.AddNodeAsync(order);

            var kitchens = await _repositoryFactory.GetRepository<Kitchen>().GetNodesAsync();
            var randomKitchen = kitchens[new Random().Next(0, kitchens.Count)];
            //var randomKitchen = await _repositoryFactory.GetRepository<Kitchen>().GetNodeAsync("82e5e232-1987-4dcd-bd3a-8841e3f7707b");


            var deliveryMen = await _repositoryFactory.GetRepository<DeliveryMan>().GetNodesAsync();
            var randomDelMan = deliveryMen[new Random().Next(0, deliveryMen.Count)];
            //var randomdelMan = await _repositoryFactory.GetRepository<DeliveryMan>().GetNodeAsync("7ac11adb-3631-4e01-af74-eeeb7287a034");

            foreach (var dish in dishes)
            {
                await orderRepo.RelateNodesAsync(new OrderedDish() { NodeFrom = order, NodeTo = dish, Count = dishesCounts[dish.Id.ToString()] });
            }

            await orderRepo.RelateNodesAsync(new CookedBy() { NodeFrom = randomKitchen, NodeTo = order });

            await orderRepo.RelateNodesAsync(new DeliveredBy() { NodeFrom = randomDelMan, NodeTo = order });

            var firstState = ObjectCache<OrderState>.Instance.First(h => h.NumberOfStage == (int)OrderStateEnum.InQueue);
            var relationCancel = new HasOrderState() { Comment = comment, NodeFromId = order.Id, NodeToId = firstState.Id, TimeStartState = DateTime.Now };

            order.Story.Add(relationCancel);
            await orderRepo.RelateNodesAsync(relationCancel);
            await orderRepo.UpdateNodeAsync(order);

            if (userId != null)
                await orderRepo.RelateNodesAsync(new Ordered() { NodeFromId = Guid.Parse(userId), NodeTo = order });
        }
    }
}
