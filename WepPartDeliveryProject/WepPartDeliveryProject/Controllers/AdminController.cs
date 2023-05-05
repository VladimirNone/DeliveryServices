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
    [Authorize(Roles = "Admin")]
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

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers(int page = 0)
        {
            var users = await _repositoryFactory.GetRepository<User>().GetNodesAsync(_appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);

            var pageEnded = users.Count() < 4;

            return Ok(new { users = users.GetRange(0, users.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : users.Count), pageEnded });
        }

        [HttpGet("getRoles")]
        public IActionResult GetRoles()
        {
            return Ok(((IUserRepository)_repositoryFactory.GetRepository<User>()).UserRolePriority.Keys.ToList());
        }

        [HttpPost("addUserRole")]
        public async Task<IActionResult> AddUserRole(ManipulateUserDataInDTO inputData)
        {
            var user = await _repositoryFactory.GetRepository<User>().GetNodeAsync(inputData.UserId);
            if(await _repositoryFactory.GetRepository<User>().HasNodeType(user.Id.ToString(), inputData.ChangeRole))
            {
                return BadRequest("Пользователь уже имеет роль " + inputData.ChangeRole);
            }

            await _repositoryFactory.GetRepository<User>().SetNewNodeType(user.Id.ToString(), inputData.ChangeRole);

            return Ok();
        }

        [HttpPost("removeUserRole")]
        public async Task<IActionResult> RemoveUserRole(ManipulateUserDataInDTO inputData)
        {
            var user = await _repositoryFactory.GetRepository<User>().GetNodeAsync(inputData.UserId);
            if (await _repositoryFactory.GetRepository<User>().HasNodeType(user.Id.ToString(), inputData.ChangeRole))
            {
                await _repositoryFactory.GetRepository<User>().RemoveNodeType(user.Id.ToString(), inputData.ChangeRole);

                return Ok();
            }

            return BadRequest("Пользователь еще не имеет роль " + inputData.ChangeRole);
        }
    }
}
