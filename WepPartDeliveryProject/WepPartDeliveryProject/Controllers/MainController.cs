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

        public MainController(IRepositoryFactory repositoryFactory, IOptions<ApplicationSettings> configOptions)
        {
            _repositoryFactory = repositoryFactory;
            _appSettings = configOptions.Value;
        }

        [HttpGet("getCategoriesList")]
        public async Task<IActionResult> GetCategoriesList()
        {
            var categories = Category.CategoriesFromDb;
            return Ok(categories);
        }

        [HttpGet("getDishesList")]
        public async Task<IActionResult> GetDishesList(int page, int categoryNumber)
        {
            if(categoryNumber < 1 || categoryNumber >= Category.CategoriesFromDb.Count)
            {
                return BadRequest($"CategoryNumber must be in range [1,{Category.CategoriesFromDb.Count}], but request contain CategoryNumber={categoryNumber}");
            }
            var choicedCategory = Category.CategoriesFromDb.Single(h=>h.CategoryNumber == categoryNumber);
            var categoryDishes = await _repositoryFactory.GetRepository<Category>().GetRelatedNodesAsync<ContainsDish, Dish>(choicedCategory, page * _appSettings.CountOfItemsOnWebPage, _appSettings.CountOfItemsOnWebPage, "Name");

            return Ok(categoryDishes);
        }
    }
}
