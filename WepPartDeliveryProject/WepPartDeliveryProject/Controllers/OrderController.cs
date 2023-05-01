using DbManager.Data.DTOs;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WepPartDeliveryProject.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;

        public OrderController(IRepositoryFactory repositoryFactory, IConfiguration configuration)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
        }

        [AllowAnonymous]
        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", res.Keys.ToArray());

            return Ok(dishes);
        }

        [HttpPost("placeAnOrder")]
        public async Task<IActionResult> PlaceAnOrder()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", res.Keys.ToArray());

            return Ok();
        }

        [HttpGet("getOrderList")]
        public async Task<IActionResult> GetOrderList()
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            //var user = _mapper.Map<UserOutDTO>(await _repositoryFactory.GetRepository<User>().GetNodeAsync(userId));

            return Ok();
        }
    }
}
