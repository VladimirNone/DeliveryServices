using AutoMapper;
using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WepPartDeliveryProject.Controllers
{
    //[Authorize(Roles = "KitchenWorker")]
    [Route("[controller]")]
    [ApiController]
    public class KitchenController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;

        public KitchenController(IRepositoryFactory repositoryFactory, IConfiguration configuration, IMapper mapper)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("getOrderStates")]
        public async Task<IActionResult> GetOrderStates()
        {
            var states = _mapper.Map<List<OrderStateItemOutDTO>>(OrderState.OrderStatesFromDb);

            return Ok(states);
        }

        [HttpGet("getOrders")]
        public async Task<IActionResult> GetOrderQueue(int page = 0, int numberOfState = 1)
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

            var workedIns = await _repositoryFactory.GetRepository<KitchenWorker>().GetRelationsOfNodesAsync<WorkedIn, Kitchen>(userId);
            var kitchen = (Kitchen)workedIns.FirstOrDefault().NodeTo;

            var orders = await orderRepo.GetOrdersByState(kitchen.Id.ToString(), (OrderStateEnum)numberOfState, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);

            var ordersOut = _mapper.Map<List<OrderOutDTO>>(orders);

            var pageEnded = ordersOut.Count() < 4;

            return Ok(new { orders = ordersOut.GetRange(0, ordersOut.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : ordersOut.Count), pageEnded });
        }

        [HttpGet("getWorkers")]
        public async Task<IActionResult> GeWorkers()
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var workedIns = await _repositoryFactory.GetRepository<KitchenWorker>().GetRelationsOfNodesAsync<WorkedIn, Kitchen>(userId);
            var kitchen = (Kitchen)workedIns.FirstOrDefault().NodeTo;

            var kitchenWorkers = await _repositoryFactory.GetRepository<Kitchen>().GetRelationsOfNodesAsync<WorkedIn, KitchenWorker>(kitchen);

            var kitchenWorkersOut = _mapper.Map<List<KitchenWorkerOutDTO>>(kitchenWorkers.Select(h=>(KitchenWorker)h.NodeFrom).ToList());

            for (int i = 0; i < kitchenWorkersOut.Count; i++)
            {
                if(kitchenWorkersOut[i].Id == kitchenWorkers[i].NodeFromId)
                {
                    kitchenWorkersOut[i].GotJob = kitchenWorkers[i].GotJob;
                }
            }

            return Ok(kitchenWorkersOut);
        }
    }
}
