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

        public MainController(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
        [HttpGet("getDishesList")]
        public IActionResult GetDishesList(int page)
        {
            return Ok();
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

            var order = await orderRepo.GetNodeAsync(Guid.Parse("261f62c7-284a-477b-9299-5a9996e9afa9"));

            var orderedDishes = await orderRepo.GetRelatedNodesAsync<OrderedDish, Dish>(order, orderByProperty: "Count");
            


            /*            var orderRepo = _repositoryFactory.GetRepository<Order>();
                        var clientRepo = _repositoryFactory.GetRepository<Client>();

           
            var order = await orderRepo.GetNodeAsync(Guid.Parse("3b76d755-ae98-4706-b1c5-8f0a901c7ba3"));
                        var client = await clientRepo.GetNodeAsync(Guid.Parse("ed885ac7-9ba0-4aec-996a-ce7a0451fdea"));

                        var orderedBy = await orderRepo.GetRelationOfNodesAsync<Ordered, Client>(order, client, true);*/

            /*var order = await orderRepo.GetNodeAsync(3);
            var client = await clientRepo.GetNodeAsync(1);
            await orderRepo.RelateNodesAsync<Ordered, Client>(order, new Ordered() { SomeText = "SomeTextik"}, client, true);*/

            return Ok(orderedDishes);
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateClient()
        {
            return Ok();
        }
    }
}
