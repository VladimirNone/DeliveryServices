using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WepPartDeliveryProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly DataGenerator _dataGenerator;


        public MainController(IRepositoryFactory repositoryFactory, IPasswordService passService, DataGenerator dataGenerator)
        {
            _repositoryFactory = repositoryFactory;
            _dataGenerator = dataGenerator;
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

            var orders = await orderRepo.GetNodesAsync(0,5, "Price","SumWeight");

            var ordersInQueue = await orderRepo.GetOrdersByState("31301ccc-cef5-469e-a339-ac750bac486e", OrderStateEnum.InQueue);
            var ordersCooking = await orderRepo.GetOrdersByState("31301ccc-cef5-469e-a339-ac750bac486e", OrderStateEnum.Cooking);

            var orderToNextStage = ordersInQueue.First();
            await orderRepo.MoveOrderToNextStage(orderToNextStage.Id.ToString(), null);

            var ordersInQueue1 = await orderRepo.GetOrdersByState("31301ccc-cef5-469e-a339-ac750bac486e", OrderStateEnum.InQueue);
            var ordersCooking1 = await orderRepo.GetOrdersByState("31301ccc-cef5-469e-a339-ac750bac486e", OrderStateEnum.Cooking);
            


            /*            var orderRepo = _repositoryFactory.GetRepository<Order>();
                        var clientRepo = _repositoryFactory.GetRepository<Client>();

                        var order = await orderRepo.GetNodeAsync(Guid.Parse("3b76d755-ae98-4706-b1c5-8f0a901c7ba3"));
                        var client = await clientRepo.GetNodeAsync(Guid.Parse("ed885ac7-9ba0-4aec-996a-ce7a0451fdea"));

                        var orderedBy = await orderRepo.GetRelationOfNodesAsync<Ordered, Client>(order, client, true);*/

            /*var order = await orderRepo.GetNodeAsync(3);
            var client = await clientRepo.GetNodeAsync(1);
            await orderRepo.RelateNodesAsync<Ordered, Client>(order, new Ordered() { SomeText = "SomeTextik"}, client, true);*/

            return Ok();
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateClient()
        {
            return Ok();
        }
    }
}
