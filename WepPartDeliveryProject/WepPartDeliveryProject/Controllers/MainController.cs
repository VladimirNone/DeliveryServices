using DbManager.Data.Nodes;
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
            var genRepo = _repositoryFactory.GetRepository<Client>();
            await genRepo.AddNodeAsync(new Client() { Id = 1, Name = "Koli", Address = "The walk street" });
            return Ok();
        }
    }
}
