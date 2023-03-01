using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Services;

namespace DbManager.Neo4j.DataGenerator
{
    public class DataGenerator
    {
        IPasswordService pswService { get; set; }

        public DataGenerator(IPasswordService passwordService)
        {
            pswService = passwordService;
        }

        public List<Admin> GenerateAdmins(int count)
            => ObjectGenerator.GenerateAdmin(pswService).Generate(count);

        public List<Client> GenerateClients(int count)
            => ObjectGenerator.GenerateClient(pswService).Generate(count);

        public List<DeliveryMan> GenerateDeliveryMen(int count)
            => ObjectGenerator.GenerateDeliveryMan(pswService).Generate(count);

        public List<KitchenWorker> GenerateKitchenWorkers(int count)
            => ObjectGenerator.GenerateKitchenWorker(pswService).Generate(count);
    
        public List<Dish> GenerateDishes(int count)
            => ObjectGenerator.GenerateDish().Generate(count);

        public List<Order> GenerateOrders(int count)
            => ObjectGenerator.GenerateOrder().Generate(count);

        public List<Kitchen> GenerateKitchens(int count)
            => ObjectGenerator.GenerateKitchen().Generate(count);

        public List<OrderState> GenerateOrderStates()
            => new List<OrderState> 
            {
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 1, NameOfState = "В очереди заказов", DescriptionForClient = "Заказ был получен и в текущий момент находится в очереди заказов на кухне"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 2, NameOfState = "Готовится", DescriptionForClient = "Блюда готовятся на кухне"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 3, NameOfState = "В ожидании курьера", DescriptionForClient = "Заказ собран и ожидает когда его заберет курьер"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 4, NameOfState = "Доставляется", DescriptionForClient = "Заказ находится у курьера, который его доставляет"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 5, NameOfState = "Завершен", DescriptionForClient = "Заказ был завершен"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = 6, NameOfState = "Отменён", DescriptionForClient = "Заказ был отменен"},
            };

        public List<CookedBy> GenerateCookedByRelations(int count, List<Order> orders, List<Kitchen> kitchens)
            => ObjectGenerator.GenerateCookedBy(orders, kitchens).Generate(count);

        public List<DeliveredBy> GenerateDeliveredByRelations(int count, List<Order> orders, List<DeliveryMan> deliveryMen)
            => ObjectGenerator.GenerateDeliveredBy(orders, deliveryMen).Generate(count);

        public List<HasOrderState> GenerateHasOrderStateRelations(int count, List<Order> orders, List<OrderState> orderStates)
            => ObjectGenerator.GenerateHasOrderState(orders, orderStates).Generate(count);

        public List<Ordered> GenerateOrderedRelations(int count, List<Order> orders, List<Client> client)
            => ObjectGenerator.GenerateOrdered(orders, client).Generate(count);

        public List<OrderedDish> GenerateOrderedDishRelations(int count, List<Order> orders, List<Dish> dishes)
            => ObjectGenerator.GenerateOrderedDish(orders, dishes).Generate(count);

        public List<ReviewedBy> GenerateReviewedByRelations(int count, List<Order> orders, List<Client> reviewers)
            => ObjectGenerator.GenerateReviewedBy(orders, reviewers).Generate(count);

        public List<WorkedIn> GenerateWorkedInRelations(int count, List<Kitchen> kitchens, List<KitchenWorker> kitchenWorkers)
            => ObjectGenerator.GenerateWorkedIn(kitchens, kitchenWorkers).Generate(count);
    }
}
