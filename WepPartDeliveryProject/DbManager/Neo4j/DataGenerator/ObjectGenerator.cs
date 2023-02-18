using Bogus;
using DbManager.Data.Nodes;
using DbManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Relations;

namespace DbManager.Neo4j.DataGenerator
{
    public class ObjectGenerator
    {
        public static Faker<Dish> GenerateProduct()
            => new Faker<Dish>("ru")
                .RuleFor(h => h.Name, g => g.Commerce.ProductName())
                .RuleFor(h => h.Description, g => g.Lorem.Paragraph())
                .RuleFor(h => h.DirectoryWithImages, g => "/Dishes/" + g.Random.Number(0, 10) + "/")
                .RuleFor(h => h.Price, g => Math.Round(g.Random.Double() + g.Random.Number(150, 1000), 2))
                .RuleFor(h => h.Weight, g => g.Random.Number(150, 1200));

        /*public static Faker<OrderedDish> GenerateOrderedProduct(Order[] orders, Dish[] dishes)
            => new Faker<OrderedDish>("ru")
                .RuleFor(h => h.Product, g => g.Random.ArrayElement(dishes))
                .RuleFor(h => h.Order, g => g.Random.ArrayElement(orders))
                .RuleFor(h => h.Count, g => g.Random.Number(1, 10))
                .RuleFor(h => h.TimeOfBuing, (g, o) => g.Date.Between(products.First(j => j.Id == o.Product.Id).DateOfPublication, DateTime.Now));
*/
        public static Faker<Order> GenerateOrder()
            => new Faker<Order>("ru")
                .RuleFor(h => h.DeliveryAddress, g => g.Address.StreetAddress())
                .RuleFor(h => h.WasOrdered, g => g.Date.Between(DateTime.Parse("2015.05.05"), DateTime.Parse("2015.06.05")))
                .RuleFor(h => h.StartCook, (g, o) => g.Date.Between(o.WasOrdered.Value, o.WasOrdered.Value.AddHours(1)))
                .RuleFor(h => h.WasCooked, (g, o) => g.Date.Between(o.StartCook.Value, o.StartCook.Value.AddHours(1)))
                .RuleFor(h => h.TakenByDeliveryMan, (g, o) => g.Date.Between(o.WasCooked.Value, o.WasCooked.Value.AddMinutes(30)))
                .RuleFor(h => h.WasDelivered, (g, o) => g.Date.Between(o.TakenByDeliveryMan.Value, o.TakenByDeliveryMan.Value.AddHours(1)));

/*        public static Faker<Admin> GenerateUser()
            => new Faker<Admin>("ru")
                .RuleFor(h => h.DateOfRegistration, g => g.Date.Between(new DateTime(2015, 10, 10), DateTime.Now))
                .RuleFor(h => h.Email, g => g.Person.Email)
                .RuleFor(h => h.UserName, g => g.Person.UserName)
                .RuleFor(h => h.PasswordHash, g => g.Internet.Password());
        //.FinishWith(async (g, o) => await userManager.CreateAsync(o, g.Internet.Password()));

        public static Faker<Author> GenerateAuthor()
            => new Faker<Author>("ru")
                .RuleFor(h => h.FirstName, g => g.Person.FirstName)
                .RuleFor(h => h.LastName, g => g.Person.LastName);*/
    }
}
