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

        [HttpGet("create")]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = _repositoryFactory.GetRepository<Order>();
            var clientRepo = _repositoryFactory.GetRepository<Client>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse("3b76d755-ae98-4706-b1c5-8f0a901c7ba2"));

            /*            
                        var order = await orderRepo.GetNodeAsync(3);
                        
                        var client = await clientRepo.GetNodeAsync(1);
                        await orderRepo.RelateNodes<OrderedBy, Client>(order, new OrderedBy() { SomeText = "SomeTextik"}, client, true);*/
            return Ok(order);
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateClient()
        {
            return Ok();
        }
    }
}
