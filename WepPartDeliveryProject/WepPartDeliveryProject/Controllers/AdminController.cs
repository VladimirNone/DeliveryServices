﻿using AutoMapper;
using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;


namespace WepPartDeliveryProject.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AdminController(IRepositoryFactory repositoryFactory, IConfiguration configuration, IMapper mapper)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _configuration = configuration;
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

        [HttpGet("getDish/{id}")]
        public async Task<IActionResult> GetDish(Guid id)
        {
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(id);
            var dishCategory = await _repositoryFactory.GetRepository<Dish>().GetRelationsOfNodesAsync<ContainsDish, Category>(dish);

            return Ok(new { dish, category = dishCategory.First().NodeFrom });
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
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id.ToString());
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
            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id.ToString());
            if (dish == null)
            {
                return BadRequest("Данные о блюде не обнаружены");
            }

            dish.IsAvailableForUser = !dish.IsAvailableForUser;
            await _repositoryFactory.GetRepository<Dish>().UpdateNodeAsync(dish);

            return Ok(dish.IsAvailableForUser);
        }

        [HttpPatch("changeDish")]
        public async Task<IActionResult> ChangeDish([FromForm] ManipulateDishDataInDTO inputData)
        {
            var dishRepo = _repositoryFactory.GetRepository<Dish>();
            var dishToChange = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.Id.ToString());
            if(dishToChange == null)
            {
                return BadRequest("Данные о блюде не обнаружены");
            }

            var category = Category.CategoriesFromDb.SingleOrDefault(h => h.Id.ToString() == inputData.CategoryId);

            var curDishCategory = await dishRepo.GetRelationsOfNodesAsync<ContainsDish, Category>(dishToChange);

            if(curDishCategory != null && curDishCategory.Count !=0 && curDishCategory.First().NodeFromId != category.Id)
            {
                await dishRepo.DeleteRelationOfNodesAsync<ContainsDish, Category>(dishToChange, (Category)curDishCategory.First().NodeFrom);

                await dishRepo.RelateNodesAsync(new ContainsDish() { NodeFrom = category, NodeTo = dishToChange });
            }

            _mapper.Map(inputData, dishToChange);

            var newImages = await LoadImagesToDir(inputData.ImagesFiles, category.LinkName, dishToChange.Id.ToString());
            if(newImages.Count != 0)
            {
                dishToChange.Images = newImages;
            }

            await _repositoryFactory.GetRepository<Dish>().UpdateNodeAsync(dishToChange);

            return Ok(dishToChange.Id);
        }

        [HttpPost("createDish")]
        public async Task<IActionResult> CreateDish([FromForm] ManipulateDishDataInDTO inputData)
        {
            var dishRepo = _repositoryFactory.GetRepository<Dish>();
            var dish = _mapper.Map<Dish>(inputData);
            dish.Images = new List<string>();
            var category = Category.CategoriesFromDb.SingleOrDefault(h => h.Id.ToString() == inputData.CategoryId);

            await dishRepo.AddNodeAsync(dish);
            await dishRepo.RelateNodesAsync(new ContainsDish() { NodeFrom = category, NodeTo = dish});

            dish.Images = await LoadImagesToDir(inputData.ImagesFiles, category.LinkName, dish.Id.ToString());
            if(dish.Images.Count != 0)
            {
                await dishRepo.UpdateNodeAsync(dish);
            }

            return Ok(dish.Id);
        }

        private async Task<List<string>> LoadImagesToDir(IFormFileCollection? files, string categoryLinkName, string dishId)
        {
            var images = new List<string>();

            var pathToPublicClientAppDirectory = _configuration.GetSection("ClientAppSettings:PathToPublicSourceDirecroty").Value;
            var dirWithDishImages = _configuration.GetSection("ClientAppSettings:DirectoryWithDishImages").Value;

            var pathToDishesDir = Path.Combine(pathToPublicClientAppDirectory, dirWithDishImages);
            var pathToCategoryDir = Path.Combine(pathToDishesDir, categoryLinkName);
            var pathToDishDir = Path.Combine(pathToCategoryDir, dishId);

            if(!Directory.Exists(pathToDishDir))
                Directory.CreateDirectory(pathToDishDir);

            if (files != null)
            {
                foreach (var imageFile in files)
                {
                    var pathToImage = Path.Combine(pathToDishDir, imageFile.FileName);
                    using var fileStream = new FileStream(pathToImage, FileMode.Create);
                    await imageFile.CopyToAsync(fileStream);

                    images.Add(Path.Combine("/", pathToImage.Replace(pathToPublicClientAppDirectory, "").Replace('\\', '/')));
                }
            }

            return images;
        }
    }
}