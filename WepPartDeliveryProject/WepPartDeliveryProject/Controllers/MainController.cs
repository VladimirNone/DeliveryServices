using AutoMapper;
using DbManager.Data;
using DbManager.Data.Cache;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMapper _mapper;
        private readonly int countCharInDishDescription = 160;

        public MainController(IRepositoryFactory repositoryFactory, IMapper mapper, IOptions<ApplicationSettings> appSettingsOptions)
        {
            // Fetch settings object from configuration
            _appSettings = appSettingsOptions.Value;

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        [HttpGet("getDishesForMainPage")]
        public async Task<IActionResult> GetDishesForMainPage(int page = 0)
        {
            //обычному пользователю не должен быть доступен удаленный или недоступный продукт
            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesAsync(_appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1, "Name");

            var pageEnded = dishes.Count < _appSettings.CountOfItemsOnWebPage + 1;

            PrepareDish(dishes);

            return Ok(new { dishes, pageEnded });
        }

        [HttpGet("getOrderStates")]
        public IActionResult GetOrderStates()
        {
            return Ok(_mapper.Map<List<OrderStateItemOutDTO>>(ObjectCache<OrderState>.Instance.ToList()));
        }

        [HttpGet("getCategoriesList")]
        public IActionResult GetCategoriesList()
        {
            return Ok(ObjectCache<Category>.Instance.ToList());
        }

        [HttpGet("getDishIds")]
        public async Task<IActionResult> GetDishIds()
        {
            return Ok((await _repositoryFactory.GetRepository<Dish>().GetNodesAsync()).Select(h => h.Id));
        }

        [HttpGet("getDish/{id}")]
        public async Task<IActionResult> GetDish(Guid id)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(id);

            if (dish.IsDeleted || !dish.IsAvailableForUser)
            {
                return BadRequest("Данный продукт был скрыт или удален");
            }

            return Ok(await Task.FromResult(dish));
        }

        [HttpGet("getDishAbilityInfo/{id}")]
        public async Task<IActionResult> GetDishAbilityInfo(Guid id)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(id);

            var userCanAbilityToViewDish = !dish.IsDeleted && dish.IsAvailableForUser;

            return Ok(await Task.FromResult(userCanAbilityToViewDish));
        }

        [HttpGet("getDishesList/{category}")]
        public async Task<IActionResult> GetDishesList(string category)
        {
            var choicedCategory = ObjectCache<Category>.Instance.ToList().Single(h=>h.LinkName == category);
            var categoryDishes = await _repositoryFactory.GetRepository<Category>().GetRelationsOfNodesAsync<ContainsDish, Dish>(choicedCategory, orderByProperty: "Name");

            var dishes = categoryDishes.Where(h => !((Dish)h.NodeTo).IsDeleted && ((Dish)h.NodeTo).IsAvailableForUser).Select(h => (Dish)h.NodeTo).ToList();

            PrepareDish(dishes);

            return Ok(dishes);
        }

        [HttpGet("getSearchedDishes")]
        public async Task<IActionResult> GetCart(string searchText, int page = 0)
        {
            //обычному пользователю не должен быть доступен удаленный или недоступный продукт
            var dishes = await ((IDishRepository)_repositoryFactory.GetRepository<Dish>())
                .SearchDishesByNameAndDescription(searchText, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1, "Name");

            var pageEnded = dishes.Count() < _appSettings.CountOfItemsOnWebPage + 1;

            PrepareDish(dishes);

            return Ok(new { dishes, pageEnded});
        }

        [Authorize]
        [HttpGet("getProfileInfo")]
        public async Task<IActionResult> GetProfileInfo()
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var user = _mapper.Map<ProfileUserOutDTO>(await _repositoryFactory.GetRepository<User>().GetNodeAsync(userId));

            return Ok(user);
        }

        private void PrepareDish(List<Dish> dishes)
        {
            for (int i = 0; i < dishes.Count; i++)
            {
                if (dishes[i].Description.Length > countCharInDishDescription)
                    dishes[i].Description = string.Concat(dishes[i].Description.AsSpan(0, countCharInDishDescription - 3), "...");
            }
        }
    }
}
