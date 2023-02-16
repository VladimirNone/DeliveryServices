using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
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

        [HttpGet]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = _repositoryFactory.GetRepository<Order>();
            var order = await orderRepo.GetNodeAsync(3);
            var clientRepo = _repositoryFactory.GetRepository<Client>();
            var client = await clientRepo.GetNodeAsync(1);
            await orderRepo.RelateNodes<OrderedBy, Client>(order, new OrderedBy() { SomeText = "SomeTextik"}, client, true);
            return Ok();
        }
    }
}
