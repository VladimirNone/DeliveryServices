
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Newtonsoft.Json;

namespace DbManager.Neo4j.DataGenerator
{
    public class GeneratorService
    {
        private readonly IRepositoryFactory _repoFactory;
        private readonly DataGenerator _dataGenerator;

        public GeneratorService(IRepositoryFactory repositoryFactory, DataGenerator dataGenerator)
        {
            _repoFactory = repositoryFactory;
            _dataGenerator = dataGenerator;
        }

        public async Task GenerateAll()
        {
            var mediumCountDishesInOrder = 3;

            //генерируем узлы
            var orderStates = _dataGenerator.GenerateOrderStates();
            var dishes = _dataGenerator.GenerateDishes(30);
            var admins = _dataGenerator.GenerateAdmins(2);
            var clients = _dataGenerator.GenerateClients(20);
            var deliveryMen = _dataGenerator.GenerateDeliveryMen(5);
            var kitchenWorkers = _dataGenerator.GenerateKitchenWorkers(9);
            var kitchens = _dataGenerator.GenerateKitchens(3);
            var orders = _dataGenerator.GenerateOrders(30);

            //вставляем узлы в бд
            var orderRepo = _repoFactory.GetRepository<Order>();
            await orderRepo.AddNodesAsync(orders);
            await _repoFactory.GetRepository<Dish>().AddNodesAsync(dishes);
            await _repoFactory.GetRepository<Admin>().AddNodesAsync(admins);
            await _repoFactory.GetRepository<Client>().AddNodesAsync(clients);
            await _repoFactory.GetRepository<DeliveryMan>().AddNodesAsync(deliveryMen);
            await _repoFactory.GetRepository<KitchenWorker>().AddNodesAsync(kitchenWorkers);
            await _repoFactory.GetRepository<Kitchen>().AddNodesAsync(kitchens);
            await _repoFactory.GetRepository<OrderState>().AddNodesAsync(orderStates);

            //генерируем связи между узлами. Последовательность важна!
            var workedIns = _dataGenerator.GenerateRelationsWorkedIn(kitchenWorkers.Count, kitchens, new List<KitchenWorker>(kitchenWorkers));
            var cookedBies = _dataGenerator.GenerateRelationsCookedBy(orders.Count, new List<Order>(orders), kitchens);
            var deliveredBies = _dataGenerator.GenerateRelationsDeliveredBy(orders.Count, new List<Order>(orders), deliveryMen);
            var hasOrderStates = _dataGenerator.GenerateRelationsHasOrderState(orders.Count, new List<Order>(orders), orderStates);
            var ordereds = _dataGenerator.GenerateRelationsOrdered(orders.Count, new List<Order>(orders), clients);
            var reviewedBies = new List<ReviewedBy>();

            var finishedOrders = hasOrderStates.Where(h => ((OrderState)h.NodeTo).NumberOfStage == (int)OrderStateEnum.Finished).Select(h => (Order)h.NodeFrom).ToList();

            //Генерируем связь ReviewedBy между клиентами и ИХ заказами. Генерация только для завершенных заказов
            foreach (var client in clients)
            {
                var orderedOrders = ordereds
                    .Where(h=>h.NodeFrom.Id == client.Id && finishedOrders.Contains((Order)h.NodeTo))
                    .Select(h=>(Order)h.NodeTo)
                    .ToList();

                reviewedBies.AddRange(_dataGenerator
                    .GenerateRelationsReviewedBy(orderedOrders.Count, new List<Order>(orderedOrders), new List<Client>() { client }));
            }
            //Генерируем связь OrderedDish. Количество связей будет меньше, т.к. удаляются дублируемые
            var orderedDishes = _dataGenerator.GenerateRelationsOrderedDish(orders.Count * mediumCountDishesInOrder, orders, dishes);
            
            //связываем узлы
            foreach (var item in workedIns)
                await _repoFactory.GetRepository<Kitchen>().RelateNodesAsync(item);

            for (int i = 0; i < orders.Count; i++)
            {
                //обновляем узлы Order, т.к. появились привязанные заказы
                await orderRepo.UpdateNodeAsync(orders[i]);
                await orderRepo.RelateNodesAsync(cookedBies[i]);
                await orderRepo.RelateNodesAsync(deliveredBies[i]);
                await orderRepo.RelateNodesAsync(hasOrderStates[i]);
                await orderRepo.RelateNodesAsync(ordereds[i]);
            }

            foreach (var item in orderedDishes)
                await orderRepo.RelateNodesAsync(item);

            foreach (var item in reviewedBies)
                await orderRepo.RelateNodesAsync(item);
        }
    }
}
