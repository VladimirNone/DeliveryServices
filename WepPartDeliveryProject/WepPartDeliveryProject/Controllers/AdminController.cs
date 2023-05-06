using AutoMapper;
using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WepPartDeliveryProject.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;

        public AdminController(IRepositoryFactory repositoryFactory, IConfiguration configuration, IMapper mapper)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        [HttpGet("getOrders")]
        public async Task<IActionResult> GetOrders(int page = 0)
        {
            var orders = await _repositoryFactory.GetRepository<Order>().GetNodesAsync(_appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);
            
            var ordersDTOs = orders.Select(h=>_mapper.Map<OrderOutDTO>(h)).ToList();

            var pageEnded = ordersDTOs.Count() < 4;

            return Ok(new { orders = ordersDTOs.GetRange(0, ordersDTOs.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : ordersDTOs.Count), pageEnded });
        }

        [HttpGet("getDishes")]
        public async Task<IActionResult> GetDishes(string searchText = "", int page = 0)
        {
            var dishes = await ((IDishRepository)_repositoryFactory.GetRepository<Dish>(true))
                .SearchDishesByNameAndDescription(searchText, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1, "Name");

            var pageEnded = dishes.Count() < 4;

            return Ok(new { dishes = dishes.GetRange(0, dishes.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : dishes.Count), pageEnded });
        }

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers(string searchText = "", int page = 0)
        {
            var usersAsTuples = await ((IUserRepository)_repositoryFactory.GetRepository<User>(true)).SearchUsersByIdAndLoginForAdmin(searchText, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);

            var rightUsers = usersAsTuples.Select(h => _mapper.Map(h.Item2, _mapper.Map<UserForAdminOutDTO>(h.Item1))).ToList();
            
            var pageEnded = rightUsers.Count() < 4;

            return Ok(new { users = rightUsers.GetRange(0, rightUsers.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : rightUsers.Count), pageEnded });
        }

        [HttpGet("getRoles")]
        public IActionResult GetRoles()
        {
            return Ok(((IUserRepository)_repositoryFactory.GetRepository<User>(true)).UserRolePriority.Keys.ToList());
        }

        [HttpPost("addUserRole")]
        public async Task<IActionResult> AddUserRole(List<ManipulateUserDataInDTO> inputData)
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>(true);
            var usersDicrionary = (await userRepo.GetNodesByPropertyAsync("Id", inputData.Select(h => h.UserId).ToArray())).ToDictionary(h => h.Id.ToString());

            foreach (var manipulateUsersData in inputData)
            {
                //на тот случай, если не все id верно переданы
                if (usersDicrionary.TryGetValue(manipulateUsersData.UserId, out var user))
                {
                    await _repositoryFactory.GetRepository<User>().SetNewNodeType(user.Id.ToString(), manipulateUsersData.ChangeRole);
                }

            }

            return Ok();
        }

        [HttpPost("removeUserRole")]
        public async Task<IActionResult> RemoveUserRole(List<ManipulateUserDataInDTO> inputData)
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>(true);
            var usersDicrionary = (await userRepo.GetNodesByPropertyAsync("Id", inputData.Select(h => h.UserId).ToArray())).ToDictionary(h=>h.Id.ToString());

            foreach (var manipulateUsersData in inputData)
            {
                //на тот случай, если не все id верно переданы
                if(usersDicrionary.TryGetValue(manipulateUsersData.UserId, out var user))
                {
                    await _repositoryFactory.GetRepository<User>().RemoveNodeType(user.Id.ToString(), manipulateUsersData.ChangeRole);
                }

            }

            return Ok();
        }

        [HttpPost("blockUsers")]
        public async Task<IActionResult> BlockUsers(List<ManipulateUserDataInDTO> inputData)
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>(true);
            var users = await userRepo.GetNodesByPropertyAsync("Id", inputData.Select(h=>h.UserId).ToArray());
            foreach (var user in users)
            {
                user.IsBlocked = true;
                await userRepo.UpdateNodeAsync(user);
            }

            return Ok();
        }

        [HttpPost("unblockUsers")]
        public async Task<IActionResult> UnblockUsers(List<ManipulateUserDataInDTO> inputData)
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>(true);
            var users = await userRepo.GetNodesByPropertyAsync("Id", inputData.Select(h => h.UserId).ToArray());
            foreach (var user in users)
            {
                user.IsBlocked = false;
                await userRepo.UpdateNodeAsync(user);
            }

            return Ok();
        }

        [HttpPost("changeDeleteStatusOfDish")]
        public async Task<IActionResult> ChangeDeleteStatusOfDish(ManipulateDishDataInDTO inputData)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id);
            if (dish == null)
            {
                return BadRequest("Данные о блюде не обнаружены");
            }

            dish.IsDeleted = !dish.IsDeleted;
            await _repositoryFactory.GetRepository<Dish>().UpdateNodeAsync(dish);

            return Ok(dish.IsDeleted);
        }

        [HttpPost("changeVisibleStatusOfDish")]
        public async Task<IActionResult> ChangeVisibleStatusOfDish(ManipulateDishDataInDTO inputData)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id);
            if (dish == null)
            {
                return BadRequest("Данные о блюде не обнаружены");
            }

            dish.IsAvailableForUser = !dish.IsAvailableForUser;
            await _repositoryFactory.GetRepository<Dish>().UpdateNodeAsync(dish);

            return Ok(dish.IsAvailableForUser);
        }

        [HttpPost("changeDish")]
        public async Task<IActionResult> ChangeDish(ManipulateDishDataInDTO inputData)
        {
            var dishToChange = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id);
            if(dishToChange == null)
            {
                return BadRequest("Данные о блюде не обнаружены");
            }

            _mapper.Map(inputData, dishToChange);

            await _repositoryFactory.GetRepository<Dish>().UpdateNodeAsync(dishToChange);

            return Ok();
        }

        [HttpPost("createDish")]
        public async Task<IActionResult> CreateDish(ManipulateDishDataInDTO inputData)
        {
            var dish = _mapper.Map<Dish>(inputData);

            await _repositoryFactory.GetRepository<Dish>().AddNodeAsync(dish);

            return Ok(dish.Id);
        }
    }
}
