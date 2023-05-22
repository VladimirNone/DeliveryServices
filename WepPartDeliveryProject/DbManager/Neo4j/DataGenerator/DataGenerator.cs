using Bogus;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Services;
using Microsoft.Extensions.Configuration;

namespace DbManager.Neo4j.DataGenerator
{
    public class DataGenerator
    {
        private IPasswordService pswService { get; set; }
        private IConfiguration _configuration { get; set; }


        public DataGenerator(IPasswordService passwordService, IConfiguration configuration)
        {
            pswService = passwordService;
            _configuration = configuration;
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

        public List<Dish> GenerateDishes(int count, List<string> dishNames)
            => ObjectGenerator.GenerateDish(dishNames).Generate(count);

        public List<Order> GenerateOrders(int count)
            => ObjectGenerator.GenerateOrder().Generate(count);

        public List<Kitchen> GenerateKitchens(int count)
            => ObjectGenerator.GenerateKitchen().Generate(count);

        public List<Category> GenerateCategories(int count)
            => ObjectGenerator.GenerateCategory().Generate(count);

        public List<OrderState> GenerateOrderStates()
            => new List<OrderState> 
            {
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.InQueue, NameOfState = "В очереди заказов", DescriptionForClient = "Заказ был получен и в текущий момент находится в очереди заказов на кухне"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.Cooking, NameOfState = "Готовится", DescriptionForClient = "Блюда готовятся на кухне"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.WaitDeliveryMan, NameOfState = "В ожидании курьера", DescriptionForClient = "Заказ собран и ожидает когда его заберет курьер"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.Delivering, NameOfState = "Доставляется", DescriptionForClient = "Заказ находится у курьера, который его доставляет"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.Finished, NameOfState = "Завершен", DescriptionForClient = "Заказ был завершен"},
                new OrderState() { Id = Guid.NewGuid(), NumberOfStage = (int)OrderStateEnum.Cancelled, NameOfState = "Отменён", DescriptionForClient = "Заказ был отменен"},
            };

        public List<CookedBy> GenerateRelationsCookedBy(int count, List<Order> orders, List<Kitchen> kitchens)
            => ObjectGenerator.GenerateCookedBy(orders, kitchens).Generate(count);

        public List<ContainsDish> GenerateRelationsContainsDishWithNodes(int countDish, int countCategory, List<Category> categories, List<Dish> dishes)
        {
            var relations = new List<ContainsDish>();
            var mediumCountDishesInCategory = countDish / countCategory;
            var random = new Random();

            categories.AddRange(GenerateCategories(countCategory));

            for (int i = 0, j = 1; i < categories.Count; i++, j = (int)Math.Pow(2, i))
            {
                categories[i].CategoryNumber = j;
                var categoryDishNames = CategoryLinkWithDishNames[categories[i].LinkName];
                var categoryDishes = GenerateDishes(random.Next(2, mediumCountDishesInCategory + 2), categoryDishNames);
                dishes.AddRange(categoryDishes);

                var pathToPublicClientAppDirectory = _configuration.GetSection("ClientAppSettings:PathToPublicSourceDirecroty").Value;
                var dirWithDishImages = _configuration.GetSection("ClientAppSettings:DirectoryWithDishImages").Value;
                var dirWithImagesForGeneration = Path.Combine("ImagesForGeneration", categories[i].LinkName);
                var filesForGeneration = Directory.GetFiles(dirWithImagesForGeneration);

                for (int l = 0; l < filesForGeneration.Length; l++)
                {
                    var pathToDishDir = ServiceRegistration.PathToDirWithDish(  pathToPublicClientAppDirectory, 
                                                                                dirWithDishImages, 
                                                                                categories[i].LinkName, 
                                                                                categoryDishes[l % categoryDishes.Count].Id.ToString());
                    var imageName = filesForGeneration[l].Replace(dirWithImagesForGeneration+"\\", "");
                    var pathToDishFile = Path.Combine(pathToDishDir, imageName);

                    if (!Directory.Exists(pathToDishDir))
                        Directory.CreateDirectory(pathToDishDir);

                    File.Copy(filesForGeneration[l], pathToDishFile, false);

                    if (categoryDishes[l % categoryDishes.Count].Images == null)
                        categoryDishes[l % categoryDishes.Count].Images = new List<string>();

                    categoryDishes[l % categoryDishes.Count].Images.Add(ServiceRegistration.ConvertFromIOPathToInternetPath_DirWithDish(pathToPublicClientAppDirectory, pathToDishFile));
                }

                var categoryContainsDishRelation = ObjectGenerator.GenerateContainsDish(new List<Category>() { categories[i] }, categoryDishes)
                    .Generate(categoryDishes.Count);

                relations.AddRange(categoryContainsDishRelation);
            }

            return relations;
        }

        public List<DeliveredBy> GenerateRelationsDeliveredBy(int count, List<Order> orders, List<DeliveryMan> deliveryMen)
            => ObjectGenerator.GenerateDeliveredBy(orders, deliveryMen).Generate(count);

        public List<HasOrderState> GenerateRelationsHasOrderState(int count, List<Order> orders, List<OrderState> orderStates)
            => ObjectGenerator.GenerateHasOrderState(orders, orderStates).Generate(count);

        public List<HasOrderState> GenerateOrderStory(List<HasOrderState> currentOrderStates, List<OrderState> orderStates)
        {
            foreach (var curOrderState in currentOrderStates)
            {
                var order = (Order)curOrderState.NodeFrom;
                var state = (OrderState)curOrderState.NodeTo;
                var faker = new Faker("ru");
                var previousStageTimeStart = curOrderState.TimeStartState.AddHours(-5);
                //если заказ отменен, то он может быть отменен только до завершения заказа
                var limitStory = state.NumberOfStage == (int)OrderStateEnum.Cancelled ? (int)OrderStateEnum.Finished : state.NumberOfStage;

                for (int i = 0, j = 1; j < limitStory; i++, j = (int)Math.Pow(2,i))
                {
                    if(j != (int)OrderStateEnum.InQueue && state.NumberOfStage == (int)OrderStateEnum.Cancelled)
                    {
                        //с вероятностью 1/3 прекращаем создавать историю, тем самым
                        //отменяя заказ после случайной стадии
                        var rand = new Random().Next(0, 3);
                        if(rand == 0)
                        {
                            break;
                        }
                    }

                    var orderStoryState = new HasOrderState()
                    {
                        TimeStartState = faker.Date.Between(previousStageTimeStart, previousStageTimeStart.AddHours(1)),
                        Comment = faker.Lorem.Sentence(),
                        Id = Guid.NewGuid(),
                        NodeFromId = order.Id,
                        NodeToId = orderStates[i].Id,
                    };
                    order.Story.Add(orderStoryState);
                    previousStageTimeStart = orderStoryState.TimeStartState;
                }

                order.Story.Add(new HasOrderState()
                {
                    TimeStartState = curOrderState.TimeStartState,
                    Comment = faker.Lorem.Sentence(),
                    Id = curOrderState.Id,
                    NodeFromId = order.Id,
                    NodeToId = state.Id,
                });
            }
            return currentOrderStates;
        }

        public List<Ordered> GenerateRelationsOrdered(int count, List<Order> orders, List<Client> client)
            => ObjectGenerator.GenerateOrdered(orders, client).Generate(count);

        public List<OrderedDish> GenerateRelationsOrderedDish(int count, List<Order> orders, List<Dish> dishes)
        {
            var mediumCountDishesInOrder = count / orders.Count;
            var rand = new Random();
            List<OrderedDish> relations = new List<OrderedDish>();

            foreach (var order in orders)
            {
                var countDishInOrder = rand.Next(1, mediumCountDishesInOrder + 2);
                relations.AddRange(ObjectGenerator.GenerateOrderedDish(new List<Order>() { order }, dishes).Generate(countDishInOrder));
            }

            relations = ExcludeDuplicate(relations);

            for (int i = 0; i < relations.Count; i++)
            {
                var orderItem = (Order)relations[i].NodeFrom;
                var dishItem = (Dish)relations[i].NodeTo;

                orderItem.SumWeight += dishItem.Weight * relations[i].Count;
                orderItem.Price += dishItem.Price * relations[i].Count;
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
                    if (relations[i].NodeFrom.Id == relations[j].NodeFrom.Id && relations[i].NodeTo.Id == relations[j].NodeTo.Id)
                    {
                        relationsForRemove.Add(relations[j]);
                    }
                    else break;
                }
            }

            relations = relations.Except(relationsForRemove).ToList();

            return relations;
        }

        private Dictionary<string, List<string>> CategoryLinkWithDishNames { get; set; } =
            new Dictionary<string, List<string>>()
            {
                {"Rolls", new List<string>{ "Суши с лососем", "Унаги ролл", "Калифорния ролл", "Филадельфия ролл", "Авокадо ролл", "Креветка ролл", "Темпура ролл", "Дракон ролл", "Спайси тунец ролл", "Ролл с огурцом" } },
                {"Drinks", new List<string>{ "Мохито", "Пина Колада", "Дайкири", "Маргарита", "Май Тай", "Космополитен", "Мартини", "Белый Русский", "Текила Санрайз" } },
                {"SecondMeal", new List<string>{ "Стейк", "Котлеты", "Рыба запеченная в духовке", "Жаркое", "Гуляш", "Курица табака", "Мясо по-французски", "Свинина в соусе", "Телятина с овощами", "Гриль" } },
                {"FirstMeal", new List<string>{ "Борщ", "Солянка", "Уха", "Грибной суп", "Щи", "Окрошка", "Рассольник", "Лапша по-флотски", "Французский луковый суп", "Куриный суп с лапшой" } },
                {"Salads", new List<string>{ "Цезарь", "Греческий", "Оливье", "Винегрет", "Салат нисуаз", "Мимоза", "Тушёнка", "Капустный", "Селёдочный", "Крабовый" } },
                {"Pizza", new List<string>{ "Маргарита", "Пепперони", "Гавайская пицца", "Пицца четыре сыра", "Вегетарианская пицца", "Барбекю", "Мексиканская пицца", "Мясная пицца", "Морская пицца", "Дьябло" } },
            };
    }
}
