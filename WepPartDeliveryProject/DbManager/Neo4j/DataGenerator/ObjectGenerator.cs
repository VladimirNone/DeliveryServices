using Bogus;
using DbManager.Data.Nodes;
using DbManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Relations;
using DbManager.Services;

namespace DbManager.Neo4j.DataGenerator
{
    internal static class ObjectGenerator
    {
        private static List<string> CategoryNames = new List<string>
        {
            "Роллы",
            "Напитки",
            "Вторые блюда",
            "Первые блюда",
            "Салаты",
            "Пицца",
        };

        private static Dictionary<string, string> CategoryLinks = new Dictionary<string, string>
        {
            { "Роллы", "Rolls" },
            { "Напитки", "Drinks" },
            { "Вторые блюда", "SecondMeal" },
            { "Первые блюда", "FirstMeal" },
            { "Салаты", "Salads" },
            { "Пицца", "Pizza" },
        };

        //----------------------------------------------GenerateNodes------------------------------------------------------------

        public static Faker<Dish> GenerateDish()
            => new Faker<Dish>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Name, g => g.Commerce.ProductName())
                .RuleFor(h => h.Description, g => g.Lorem.Paragraph())
                .RuleFor(h => h.Price, g => g.Random.Number(100, 1200))
                .RuleFor(h => h.Weight, g => g.Random.Number(150, 1200));

        public static Faker<Category> GenerateCategory()
            => new Faker<Category>("ru")
                .RuleFor(h => h.Name, g => g.Random.ListItemWithRemove(CategoryNames))
                .RuleFor(h => h.LinkName, (g, o) => CategoryLinks[o.Name])
                .RuleFor(h => h.Description, g => g.Lorem.Paragraph());

        public static Faker<Order> GenerateOrder()
            => new Faker<Order>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.DeliveryAddress, g => g.Address.StreetAddress());

        public static Faker<Kitchen> GenerateKitchen()
            => new Faker<Kitchen>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Address, g => g.Address.StreetAddress());

        public static Faker<Admin> GenerateAdmin(IPasswordService pswService)
            => new Faker<Admin>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshToken, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshTokenCreated, g => g.Date.Soon())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()).ToList());

        public static Faker<Client> GenerateClient(IPasswordService pswService)
            => new Faker<Client>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshToken, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshTokenCreated, g => g.Date.Soon())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.Bonuses, g => Math.Round(g.Random.Double() + g.Random.Number(150, 1000), 2))
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()).ToList());

        public static Faker<DeliveryMan> GenerateDeliveryMan(IPasswordService pswService)
            => new Faker<DeliveryMan>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshToken, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshTokenCreated, g => g.Date.Soon())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.MaxWeight, g => g.Random.Number(4000, 50000))
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()).ToList());

        public static Faker<KitchenWorker> GenerateKitchenWorker(IPasswordService pswService)
            => new Faker<KitchenWorker>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshToken, g => Guid.NewGuid())
                .RuleFor(h => h.RefreshTokenCreated, g => g.Date.Soon())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.JobTitle, g => g.Name.JobTitle())
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()).ToList());

        //---------------------------------------------GenerateRelations-------------------------------------------------------------

        public static Faker<OrderedDish> GenerateOrderedDish(List<Order> orders, List<Dish> dishes)
            => new Faker<OrderedDish>("ru")
                .RuleFor(h => h.NodeTo, g => g.Random.ListItem(dishes))
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(orders))
                .RuleFor(h => h.Count, g => g.Random.Number(1, 10));

        public static Faker<CookedBy> GenerateCookedBy(List<Order> orders, List<Kitchen> kitchens)
            => new Faker<CookedBy>("ru")
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(kitchens))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItemWithRemove(orders));

        public static Faker<ContainsDish> GenerateContainsDish(List<Category> categories, List<Dish> dishes)
            => new Faker<ContainsDish>("ru")
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(categories))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItemWithRemove(dishes));

        public static Faker<DeliveredBy> GenerateDeliveredBy(List<Order> orders, List<DeliveryMan> deliveryMen)
            => new Faker<DeliveredBy>("ru")
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(deliveryMen))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItemWithRemove(orders));

        public static Faker<HasOrderState> GenerateHasOrderState(List<Order> orders, List<OrderState> states)
            => new Faker<HasOrderState>("ru")
                .RuleFor(h => h.TimeStartState, g => g.Date.Between(new DateTime(2022, 10, 10), new DateTime(2023, 5, 5)))
                .RuleFor(h => h.Comment, g => g.Random.Bool() ? g.Lorem.Sentence() : null)
                .RuleFor(h => h.NodeTo, g => g.Random.ListItem(states))
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItemWithRemove(orders));

        public static Faker<Ordered> GenerateOrdered(List<Order> orders, List<Client> clients)
            => new Faker<Ordered>("ru")
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(clients))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItemWithRemove(orders));

        /// <summary>
        /// Предпологается, что на вход идут только завершенные или отмененные заказы  
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="reviewers"></param>
        /// <returns></returns>
        public static Faker<ReviewedBy> GenerateReviewedBy(List<Order> orders, List<Client> reviewers)
            => new Faker<ReviewedBy>("ru")
                .RuleFor(h => h.ClientRating, g => g.Random.Int(1, 10))
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItem(reviewers))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItemWithRemove(orders))
                .RuleFor(h => h.TimeCreated, (g, o) => g.Date.Between(((Order)o.NodeTo).Story.Last().TimeStartState, ((Order)o.NodeTo).Story.Last().TimeStartState.AddHours(3)))
                .RuleFor(h => h.Review, g => g.Lorem.Paragraph());

        public static Faker<WorkedIn> GenerateWorkedIn(List<Kitchen> kitchens, List<KitchenWorker> kitchenWorkers)
            => new Faker<WorkedIn>("ru")
                .RuleFor(h => h.GotJob, g => g.Date.Between(new DateTime(2010, 10, 10), DateTime.Now))
                .RuleFor(h => h.NodeTo, g => g.Random.ListItem(kitchens))
                .RuleFor(h => h.NodeFrom, g => g.Random.ListItemWithRemove(kitchenWorkers));
    }
}
