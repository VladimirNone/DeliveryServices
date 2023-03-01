using DbManager;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Configuration;
using Neo4jClient;
using Newtonsoft.Json;
using System.Configuration;

namespace TestsForProject.TestDbManagers
{
    public class TestGeneralRepository
    {
        IGraphClient _graphClient;

        public TestGeneralRepository()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var settings = new ApplicationSettings();
            config.GetSection("TestApplicationSettings").Bind(settings);
            _graphClient = new BoltGraphClient(settings.Neo4jConnection, settings.Neo4jUser, settings.Neo4jPassword);
            _graphClient.ConnectAsync().Wait();
        }

        [Fact]
        public async Task TestGetNodeAsyncMethod()
        {
            //Test Orders
            List<Order> _orders = new List<Order>()
            {
                new Order(){Id = Guid.NewGuid(), SumWeight = 1500, Price = 850.6, Kitchen = new CookedBy(), WasOrdered = DateTime.Parse("2015.05.05"), StartCook = DateTime.Parse("2015.05.06"), DeliveryAddress = "address" }
            };

            //Add to db data
            await _graphClient.Cypher
                .Create("(order:Order)")
                .Set("order=$newOrder")
                .WithParams(new
                {
                    newOrder = _orders[0],
                })
                .ExecuteWithoutResultsAsync();

            // Arrange
            var repo = new GeneralRepository<Order>(_graphClient);

            // Act
            var result = await repo.GetNodeAsync(_orders[0].Id);

            //To string
            var resultStr = JsonConvert.SerializeObject(result);
            var modelStr = JsonConvert.SerializeObject(_orders[0]);

            //Delete data from db
            await _graphClient.Cypher
                .Match("(order:Order {Id: $id})")
                .Delete("order")
                .WithParam("id", _orders[0].Id)
                .ExecuteWithoutResultsAsync();

            // Assert
            Assert.Equal(modelStr, resultStr);
        }
    
            
    }
}
