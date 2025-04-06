using AutoMapper;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Components.Forms;

namespace WepPartDeliveryProject.Controllers
{
    [Authorize(Roles = "DeliveryMan")]
    [Route("[controller]")]
    [ApiController]
    public class DeliveryManController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;

        public DeliveryManController(IRepositoryFactory repositoryFactory, IMapper mapper, IOptions<ApplicationSettings> appSettingsOptions)
        {
            // Fetch settings object from configuration
            _appSettings = appSettingsOptions.Value;

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        [HttpGet("getOrders")]
        public async Task<IActionResult> GetOrderQueue(int page = 0, int numberOfState = 1)
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>();

            var delMan = await _repositoryFactory.GetRepository<DeliveryMan>().GetNodeAsync(userId);

            var orders = await orderRepo.GetOrdersByStateRelatedWithNode<DeliveryMan>(delMan.Id.ToString(), (OrderStateEnum)numberOfState, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);

            var ordersOut = _mapper.Map<List<OrderOutDTO>>(orders);

            var pageEnded = ordersOut.Count() < _appSettings.CountOfItemsOnWebPage + 1;

            return Ok(new { orders = ordersOut, pageEnded });
        }
    }
}
