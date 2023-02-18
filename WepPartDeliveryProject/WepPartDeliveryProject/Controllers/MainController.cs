using DbManager.Data.Nodes;
using DbManager.Data.Relations;
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

        public MainController(IRepositoryFactory repositoryFactory, IPasswordService passService)
        {
            _repositoryFactory = repositoryFactory;
            var res = passService.GetPasswordHash("12345");
            var resBool = passService.CheckPassword("12346", res);
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateClient()
        {
            var orderRepo = _repositoryFactory.GetRepository<Order>();
            var clientRepo = _repositoryFactory.GetRepository<Client>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse("3b76d755-ae98-4706-b1c5-8f0a901c7ba3"));
            var client = await clientRepo.GetNodeAsync(Guid.Parse("ed885ac7-9ba0-4aec-996a-ce7a0451fdea"));

            var orderedBy = await orderRepo.GetRelationOfNodesAsync<Ordered, Client>(order, client, true);

            /*
            var order = await orderRepo.GetNodeAsync(3);
                        
            var client = await clientRepo.GetNodeAsync(1);
            await orderRepo.RelateNodes<Ordered, Client>(order, new Ordered() { SomeText = "SomeTextik"}, client, true);*/
            return Ok(orderedBy);
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateClient()
        {
            return Ok();
        }
    }
}
