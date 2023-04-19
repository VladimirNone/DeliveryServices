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

namespace WepPartDeliveryProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IPasswordService _pswService;

        public MainController(IRepositoryFactory repositoryFactory, IOptions<ApplicationSettings> configOptions, IPasswordService passwordService)
        {
            _repositoryFactory = repositoryFactory;
            _appSettings = configOptions.Value;
            _pswService = passwordService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> GetTestList()
        {

            //похоже на получение блюд, находящихся в заказе или корзине
            /*            var orderRepo = _repositoryFactory.GetRepository<Order>();
                        var orders = await orderRepo.GetNodesAsync();
                        var order = orders.First();

                        var orderedProds = await orderRepo.GetRelatedNodesAsync<OrderedDish, Dish>(order);
                        var prods = orderedProds.Select(h => h.NodeTo).ToList();*/

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesAsync();


            return Ok();
        }

        [HttpGet("getCategoriesList")]
        public async Task<IActionResult> GetCategoriesList()
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
    }
}
 