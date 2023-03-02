using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Services;
using System.Linq;

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

        public List<CookedBy> GenerateRelationsCookedBy(int count, List<Order> orders, List<Kitchen> kitchens)
            => ObjectGenerator.GenerateCookedBy(orders, kitchens).Generate(count);

        public List<DeliveredBy> GenerateRelationsDeliveredBy(int count, List<Order> orders, List<DeliveryMan> deliveryMen)
            => ObjectGenerator.GenerateDeliveredBy(orders, deliveryMen).Generate(count);

        public List<HasOrderState> GenerateRelationsHasOrderState(int count, List<Order> orders, List<OrderState> orderStates)
        {
            var relations = ObjectGenerator.GenerateHasOrderState(orders, orderStates).Generate(count);

            //Генерация создает рандомную связь между заказом и состоянием заказа, но при этом время создается для каждого состояния
            //по этой причине, "чистим" не нужное время, чтобы оно соотвестовало состоянию
            //Например, если состояние готовится, то данные должны находиться только в StartCook и WasOrdered
            for (int i = 0; i < relations.Count; i++)
            {
                //если заказ отменен, то он мог быть отменен на любой стадии, кроме последней (Завершен - 5)
                var stage = relations[i].State.NumberOfStage == 6 ? Random.Shared.Next(1,5) : relations[i].State.NumberOfStage;

                if (stage <= 5)
                {
                    relations[i].Order.WasCancelled = null;
                    relations[i].Order.ReasonForCancellation = null;

                    if (stage <= 4)
                    {
                        relations[i].Order.WasDelivered = null;

                        if (stage <= 3)
                        {
                            relations[i].Order.TakenByDeliveryMan = null;

                            if (stage <= 2)
                            {
                                relations[i].Order.WasCooked= null;

                                if (stage <= 1)
                                {
                                    relations[i].Order.StartCook = null;
                                }
                            }
                        }
                    }
                }
            }

            return relations;
        }

        public List<Ordered> GenerateRelationsOrdered(int count, List<Order> orders, List<Client> client)
            => ObjectGenerator.GenerateOrdered(orders, client).Generate(count);

        public List<OrderedDish> GenerateRelationsOrderedDish(int count, List<Order> orders, List<Dish> dishes)
        {
            var relations = ObjectGenerator.GenerateOrderedDish(orders, dishes).Generate(count);

            relations = ExcludeDuplicate(relations);

            for (int i = 0; i < relations.Count; i++)
            {
                relations[i].Order.SumWeight += relations[i].OrderedItem.Weight * relations[i].Count;
                relations[i].Order.Price += relations[i].OrderedItem.Price * relations[i].Count;
            }
            return relations;
        }

        public List<ReviewedBy> GenerateRelationsReviewedBy(int count, List<Order> orders, List<Client> reviewers)
            => ObjectGenerator.GenerateReviewedBy(orders, reviewers).Generate(count);

        public List<WorkedIn> GenerateRelationsWorkedIn(int count, List<Kitchen> kitchens, List<KitchenWorker> kitchenWorkers)
            => ObjectGenerator.GenerateWorkedIn(kitchens, kitchenWorkers).Generate(count);

        /// <summary>
        /// Exclude Duplicate. Compare NodeFrom.Ids and NodeTo.Ids then remove this relations from list and return it
        /// </summary>
        /// <param name="relations">List with relations</param>
        /// <returns>Relations without duplicate</returns>
        private List<TRelation> ExcludeDuplicate<TRelation>(List<TRelation> relations) where TRelation: IRelation
        {
            relations = relations.OrderBy(x => x.NodeFrom.Id).ThenBy(x => x.NodeTo.Id).ToList();
            var relationsForRemove = new List<TRelation>();

            for (int i = 0; i < relations.Count - 1; i++)
            {
                for (int j = i + 1; j < relations.Count; j++)
                {
                    if (relations[j].NodeFrom.Id == relations[j].NodeFrom.Id && relations[i].NodeTo.Id == relations[j].NodeTo.Id && !relationsForRemove.Contains(relations[j]))
                    {
                        relationsForRemove.Add(relations[j]);
                    }
                    else break;
                }
            }

            relations = relations.Except(relationsForRemove).ToList();

            return relations;
        }
    }
}
