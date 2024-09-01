
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
        private readonly IPasswordService _pswService;

        public GeneratorService(IRepositoryFactory repositoryFactory, IPasswordService passwordService, DataGenerator dataGenerator)
        {
            _repoFactory = repositoryFactory;
            _dataGenerator = dataGenerator;
            _pswService = passwordService;
        }

        public async Task GenerateAll()
        {
            var mediumCountDishesInOrder = 3;
            var countRandomStateForOrders = 350;

            //генерируем узлы
            var orderStates = _dataGenerator.GenerateOrderStates();
            var admins = _dataGenerator.GenerateAdmins(2);

            var admin = admins[0];
            admin.Login = "admin@admin";
            admin.PasswordHash = _pswService.GetPasswordHash(admin.Login, "admin").ToList();

            var clients = _dataGenerator.GenerateClients(50);

            var client = clients[0];
            client.Login = "item@item";
            client.PasswordHash = _pswService.GetPasswordHash(client.Login, "item").ToList();

            var deliveryMen = _dataGenerator.GenerateDeliveryMen(7);

            var deliveryMan = deliveryMen[0];
            deliveryMan.Login = "deliveryMan@deliveryMan";
            deliveryMan.PasswordHash = _pswService.GetPasswordHash(deliveryMan.Login, "deliveryMan").ToList();

            var kitchenWorkers = _dataGenerator.GenerateKitchenWorkers(9);

            var kitchenWorker = kitchenWorkers[0];
            kitchenWorker.Login = "kitchenWorker@kitchenWorker";
            kitchenWorker.PasswordHash = _pswService.GetPasswordHash(kitchenWorker.Login, "kitchenWorker").ToList();

            var kitchens = _dataGenerator.GenerateKitchens(3);
            var orders = _dataGenerator.GenerateOrders(1000);
            var categories = new List<Category>();
            var dishes = new List<Dish>();
            //генерация категорий и блюд происходит в данном методе!
            var containsDishes = _dataGenerator.GenerateRelationsContainsDishWithNodes(30, 6, categories, dishes);

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
            await _repoFactory.GetRepository<Category>().AddNodesAsync(categories);

            //генерируем связи между узлами. Последовательность важна!
            //создаем новые списки, т.к. эти списки будут изменяться. 
            //так, при генерации связи Ordered будут удаляться заказы из списка, чтобы они не дублировались
            var workedIns = _dataGenerator.GenerateRelationsWorkedIn(kitchenWorkers.Count, kitchens, new List<KitchenWorker>(kitchenWorkers));
            var cookedBies = _dataGenerator.GenerateRelationsCookedBy(orders.Count, new List<Order>(orders), kitchens);
            var deliveredBies = _dataGenerator.GenerateRelationsDeliveredBy(orders.Count, new List<Order>(orders), deliveryMen);
            var hasOrderStates = _dataGenerator.GenerateRelationsHasOrderState(countRandomStateForOrders, new List<Order>(orders.GetRange(0, countRandomStateForOrders)), orderStates);
            hasOrderStates.AddRange(_dataGenerator.GenerateRelationsHasOrderState(orders.Count - countRandomStateForOrders, new List<Order>(orders.GetRange(countRandomStateForOrders, orders.Count - countRandomStateForOrders)), new List<OrderState>() { orderStates.First(h => h.NumberOfStage == (int)OrderStateEnum.Finished) }));
            var ordereds = _dataGenerator.GenerateRelationsOrdered(orders.Count, new List<Order>(orders), clients);
            
            //Генерируем связь OrderedDish. Количество связей будет меньше, т.к. удаляются дублируемые
            var orderedDishes = _dataGenerator.GenerateRelationsOrderedDish(orders.Count * mediumCountDishesInOrder, orders, dishes);
            
            //связываем узлы
            foreach (var item in workedIns)
                await _repoFactory.GetRepository<Kitchen>().RelateNodesAsync(item);

            //В цикле все, т.к. при генерации учитывается, что эти связи идут 1 к 2
            for (int i = 0; i < orders.Count; i++)
            {
                await orderRepo.RelateNodesAsync(cookedBies[i]);
                await orderRepo.RelateNodesAsync(deliveredBies[i]);
                await orderRepo.RelateNodesAsync(hasOrderStates[i]);
                await orderRepo.RelateNodesAsync(ordereds[i]);
            }
            //Создаем историю для заказов. Создаются рандомные связи с состояниями до последнего
            hasOrderStates = _dataGenerator.GenerateOrderStory(hasOrderStates, orderStates);

            for (int i = 0; i < orders.Count; i++)
            {
                //обновляем узлы Order, т.к. появились привязанные заказы
                await orderRepo.UpdateNodeAsync(orders[i]);
            }

            foreach (var item in orderedDishes)
                await orderRepo.RelateNodesAsync(item);

            //Создание отзыва в самом конце, т.к. время ее создания зависит от времени завершения заказа
            var reviewedBies = new List<ReviewedBy>();

            var finishedOrders = hasOrderStates.Where(h => ((OrderState)h.NodeTo).NumberOfStage == (int)OrderStateEnum.Finished).Select(h => (Order)h.NodeFrom).ToList();

            //Генерируем связь ReviewedBy между клиентами и ИХ заказами. Генерация только для завершенных заказов
            foreach (var item in clients)
            {
                var orderedOrders = ordereds
                    .Where(h => h.NodeFrom.Id == item.Id && finishedOrders.Contains((Order)h.NodeTo))
                    .Select(h => (Order)h.NodeTo)
                    .ToList();

                reviewedBies.AddRange(_dataGenerator
                    .GenerateRelationsReviewedBy(orderedOrders.Count, new List<Order>(orderedOrders), new List<Client>() { item }));
            }

            foreach (var item in reviewedBies)
                await orderRepo.RelateNodesAsync(item);

            foreach (var item in containsDishes)
                await _repoFactory.GetRepository<Dish>().RelateNodesAsync(item);

            //устанавливаем всем пользователям тип User
            foreach (var item in admins)
                await _repoFactory.GetRepository<Admin>().SetNewNodeType<User>(item.Id.ToString());
            foreach (var item in clients)
                await _repoFactory.GetRepository<Client>().SetNewNodeType<User>(item.Id.ToString());
            foreach (var item in kitchenWorkers)
                await _repoFactory.GetRepository<KitchenWorker>().SetNewNodeType<User>(item.Id.ToString());
            foreach (var item in deliveryMen)
                await _repoFactory.GetRepository<DeliveryMan>().SetNewNodeType<User>(item.Id.ToString());


        }
    }
}
