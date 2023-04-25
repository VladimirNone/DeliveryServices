using DbManager;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WepPartDeliveryProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;

        public MainController(IRepositoryFactory repositoryFactory, IConfiguration configuration)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
        }

        [HttpGet("test")]
        public async Task<IActionResult> GetTestList()
        {

            return Ok();
        }

        [HttpGet("getCategoriesList")]
        public IActionResult GetCategoriesList()
        {
            var categories = Category.CategoriesFromDb;
            return Ok(categories);
        }

        [HttpGet("getDishIds")]
        public async Task<IActionResult> GetDishIds()
        {
            var dishIds = (await _repositoryFactory.GetRepository<Dish>().GetNodesAsync()).Select(h=>h.Id).ToList();
            return Ok(dishIds);
        }

        [HttpGet("getDish/{id}")]
        public async Task<IActionResult> GetDish(Guid id)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(id);
            return Ok(dish);
        }

        [HttpGet("getDishesList/{category}")]
        public async Task<IActionResult> GetDishesList(string category)
        {
            var choicedCategory = Category.CategoriesFromDb.Single(h=>h.LinkName == category);
            var categoryDishes = await _repositoryFactory.GetRepository<Category>().GetRelatedNodesAsync<ContainsDish, Dish>(choicedCategory, orderByProperty: "Name");

            return Ok(categoryDishes.Select(h=>h.NodeTo).ToList());
        }

        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByIdAsync(res.Keys.ToArray());

            return Ok(dishes);
        }

        [HttpGet("getSearchedDishes")]
        public async Task<IActionResult> GetCart(string searchText, int page = 0)
        {
            var dishes = await ((IDishRepository)_repositoryFactory.GetRepository<Dish>(true))
                .SearchDishesByNameAndDescription(searchText, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1, "Name");

            var pageEnded = dishes.Count() < 4;

            return Ok(new { dishes = dishes.GetRange(0, dishes.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage: dishes.Count), pageEnded});
        }

        [HttpPost("placeAnOrder")]
        public async Task<IActionResult> PlaceAnOrder()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByIdAsync(res.Keys.ToArray());

            return Ok();
        }
    }
}
 