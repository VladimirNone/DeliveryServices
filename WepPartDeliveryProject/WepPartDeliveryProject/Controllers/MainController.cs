using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Implementations;
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
        public async Task<IActionResult> GetDishesList(int page, int categoryNumber)
        {
            return Ok();
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

            var order = await orderRepo.GetNodeAsync(Guid.Parse("261f62c7-284a-477b-9299-5a9996e9afa9"));

            var orderedDishes = await orderRepo.GetRelatedNodesAsync<OrderedDish, Dish>(order, orderByProperty: "Count");

            return Ok(orderedDishes);
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateClient()
        {
            return Ok();
        }
    }
}
