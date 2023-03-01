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
        //----------------------------------------------GenerateNodes------------------------------------------------------------

        public static Faker<Dish> GenerateDish()
            => new Faker<Dish>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Name, g => g.Commerce.ProductName())
                .RuleFor(h => h.Description, g => g.Lorem.Paragraph())
                .RuleFor(h => h.DirectoryWithImages, g => "/Dishes/" + g.Random.Number(0, 10) + "/")
                .RuleFor(h => h.Price, g => Math.Round(g.Random.Double() + g.Random.Number(150, 1000), 2))
                .RuleFor(h => h.Weight, g => g.Random.Number(150, 1200));

        public static Faker<Order> GenerateOrder()
            => new Faker<Order>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.DeliveryAddress, g => g.Address.StreetAddress())
                .RuleFor(h => h.WasOrdered, g => g.Date.Between(DateTime.Parse("2015.05.05"), DateTime.Parse("2015.06.05")))
                .RuleFor(h => h.StartCook, (g, o) => g.Date.Between(o.WasOrdered.Value, o.WasOrdered.Value.AddHours(1)))
                .RuleFor(h => h.WasCooked, (g, o) => g.Date.Between(o.StartCook.Value, o.StartCook.Value.AddHours(1)))
                .RuleFor(h => h.TakenByDeliveryMan, (g, o) => g.Date.Between(o.WasCooked.Value, o.WasCooked.Value.AddMinutes(30)))
                .RuleFor(h => h.WasDelivered, (g, o) => g.Date.Between(o.TakenByDeliveryMan.Value, o.TakenByDeliveryMan.Value.AddHours(1)));

        public static Faker<Kitchen> GenerateKitchen()
            => new Faker<Kitchen>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Address, g => g.Address.StreetAddress());

        public static Faker<Admin> GenerateAdmin(IPasswordService pswService)
            => new Faker<Admin>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()));

        public static Faker<Client> GenerateClient(IPasswordService pswService)
            => new Faker<Client>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.Bonuses, g => Math.Round(g.Random.Double() + g.Random.Number(150, 1000), 2))
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()));

        public static Faker<DeliveryMan> GenerateDeliveryMan(IPasswordService pswService)
            => new Faker<DeliveryMan>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.MaxWeight, g => g.Random.Number(4000, 50000))
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()));

        public static Faker<KitchenWorker> GenerateKitchenWorker(IPasswordService pswService)
            => new Faker<KitchenWorker>("ru")
                .RuleFor(h => h.Id, g => Guid.NewGuid())
                .RuleFor(h => h.Born, g => g.Date.Between(new DateTime(1980, 10, 10), new DateTime(2003, 10, 10)))
                .RuleFor(h => h.Name, g => g.Person.FullName)
                .RuleFor(h => h.Login, g => g.Internet.Email())
                .RuleFor(h => h.JobTitle, g => g.Name.JobTitle())
                .RuleFor(h => h.PhoneNumber, g => g.Phone.PhoneNumber("+7!!!!!!!!!!"))
                .RuleFor(h => h.PasswordHash, (g, o) => pswService.GetPasswordHash(o.Login, g.Internet.Password()));

        //---------------------------------------------GenerateRelations-------------------------------------------------------------

        public static Faker<OrderedDish> GenerateOrderedDish(List<Order> orders, List<Dish> dishes)
            => new Faker<OrderedDish>("ru")
                .RuleFor(h => h.OrderedItem, g => g.Random.ListItemWithRemove(dishes))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove(orders))
                .RuleFor(h => h.Count, g => g.Random.Number(1, 10));

        public static Faker<CookedBy> GenerateCookedBy(List<Order> orders, List<Kitchen> kitchens)
            => new Faker<CookedBy>("ru")
                .RuleFor(h => h.Kitchen, g => g.Random.ListItemWithRemove(kitchens))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove<Order>(orders));

        public static Faker<DeliveredBy> GenerateDeliveredBy(List<Order> orders, List<DeliveryMan> deliveryMen)
            => new Faker<DeliveredBy>("ru")
                .RuleFor(h => h.DeliveryMan, g => g.Random.ListItemWithRemove(deliveryMen))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove(orders));

        public static Faker<HasOrderState> GenerateHasOrderState(List<Order> orders, List<OrderState> states)
            => new Faker<HasOrderState>("ru")
                .RuleFor(h => h.State, g => g.Random.ListItemWithRemove(states))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove(orders));

        public static Faker<Ordered> GenerateOrdered(List<Order> orders, List<Client> clients)
            => new Faker<Ordered>("ru")
                .RuleFor(h => h.Client, g => g.Random.ListItemWithRemove(clients))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove(orders));

        public static Faker<ReviewedBy> GenerateReviewedBy(List<Order> orders, List<Client> reviewers)
            => new Faker<ReviewedBy>("ru")
                .RuleFor(h => h.Reviewer, g => g.Random.ListItemWithRemove(reviewers))
                .RuleFor(h => h.Order, g => g.Random.ListItemWithRemove(orders))
                .RuleFor(h => h.Review, g => g.Lorem.Paragraph());

        public static Faker<WorkedIn> GenerateWorkedIn(List<Kitchen> kitchens, List<KitchenWorker> kitchenWorkers)
            => new Faker<WorkedIn>("ru")
                .RuleFor(h => h.GotJob, g => g.Date.Between(new DateTime(2010, 10, 10), DateTime.Now))
                .RuleFor(h => h.Kitchen, g => g.Random.ListItemWithRemove(kitchens))
                .RuleFor(h => h.KitchenWorker, g => g.Random.ListItemWithRemove(kitchenWorkers));
    }
}
