using AutoMapper;
using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DbManager.Data.Cache;
using DbManager.Services;

namespace WepPartDeliveryProject.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;
        private readonly IOrderService _orderService;

        public OrderController(IRepositoryFactory repositoryFactory, IMapper mapper, JwtService jwtService, IOrderService orderService, IOptions<ApplicationSettings> appSettingsOptions)
        {
            // Fetch settings object from configuration
            _appSettings = appSettingsOptions.Value;

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
            _jwtService = jwtService;
            _orderService = orderService;
        }

        [AllowAnonymous]
        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = ObjectCache<Dish>.Instance.Where(h => res.Keys.Contains(h.Id.ToString()));

            return Ok(dishes);
        }

        [AllowAnonymous]
        [HttpPost("placeAnOrder")]
        public async Task<IActionResult> PlaceAnOrder(PlaceAnOrderInDTO inputData)
        {
            var jsonData = Request.Cookies["cartDishes"];
            var userId = Request.Cookies["X-UserId"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();
            if (res.Count == 0)
                return BadRequest("При оформлении заказа, в корзине отсутсвовали продукты");

            await this._orderService.PlaceAnOrder(userId, res, inputData.Comment, inputData.PhoneNumber, inputData.DeliveryAddress);

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("changeCountOrderedDish")]
        public async Task<IActionResult> ChangeCountOrderedDish(ManipulateOrderDataInDTO inputData)
        {
            await this._orderService.ChangeCountOrderedDish(inputData.OrderId, inputData.DishId, (int)inputData.NewCount);

            return Ok();
        }

        [HttpGet("getClientOrders")]
        public async Task<IActionResult> GetClientOrders(int page = 0, int numberOfState = 1)
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>();

            var orders = await orderRepo.GetOrdersByStateRelatedWithNode<Client>(userId, (OrderStateEnum)numberOfState, _appSettings.CountOfItemsOnWebPage * page, _appSettings.CountOfItemsOnWebPage + 1);

            var ordersOut = _mapper.Map<List<OrderOutDTO>>(orders);

            var pageEnded = ordersOut.Count() < _appSettings.CountOfItemsOnWebPage + 1;

            return Ok(new { orders = ordersOut.GetRange(0, ordersOut.Count > _appSettings.CountOfItemsOnWebPage ? _appSettings.CountOfItemsOnWebPage : ordersOut.Count), pageEnded });
        }

        [HttpGet("getOrder/{orderId}")]
        public async Task<IActionResult> GetOrder(string orderId)
        {
            Order? searchedOrder;
            if (_jwtService.UserHasRole(Request.Headers.Authorization.FirstOrDefault(), "Admin") 
                || _jwtService.UserHasRole(Request.Headers.Authorization.FirstOrDefault(), "KitchenWorker")
                || _jwtService.UserHasRole(Request.Headers.Authorization.FirstOrDefault(), "DeliveryMan"))
            {
                searchedOrder = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(orderId);
            }
            else
            {
                var userId = Request.Cookies["X-UserId"];

                if (userId == null)
                {
                    return BadRequest("You don't have refresh token. You need to login or signup to system");
                }

                var ordereds = await _repositoryFactory.GetRepository<Client>().GetRelationsOfNodesAsync<Ordered, Order>(userId);
                searchedOrder = (Order)ordereds.SingleOrDefault(h => h.NodeToId.ToString() == orderId)?.NodeTo;
            }

            if (searchedOrder != null)
            {
                var reviewedOrder = await _repositoryFactory.GetRepository<Order>().GetRelationsOfNodesAsync<ReviewedBy, Client>(searchedOrder);
                var orderedDishes = await _repositoryFactory.GetRepository<Order>().GetRelationsOfNodesAsync<OrderedDish, Dish>(searchedOrder);

                var preparedOrder = _mapper.Map<OrderOutDTO>(searchedOrder);
                if(reviewedOrder.Count != 0)
                    _mapper.Map(reviewedOrder[0], preparedOrder);
                preparedOrder.Story = _mapper.Map<List<OrderStateItemOutDTO>>(searchedOrder.Story);

                return Ok(new { order = preparedOrder, orderedDishes = orderedDishes.Select(h=> new {count = h.Count, dishInfo = (Dish)h.NodeTo}) });
            }

            return BadRequest("Запрашиваемый заказ не доступен данному пользователю или не существует");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("cancelOrderedDish")]
        public async Task<IActionResult> CancelOrderedDish(ManipulateOrderDataInDTO inputData)
        {
            await this._orderService.CancelOrderedDish(inputData.OrderId, inputData.DishId);

            return Ok();
        }

        [Authorize(Roles = "Admin, Client")]
        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder(ManipulateOrderDataInDTO inputData)
        {
            await this._orderService.CancelOrder(inputData.OrderId, inputData.ReasonOfCancel);

            return Ok();
        }

        [Authorize(Roles = "Admin, KitchenWorker, DeliveryMan")]
        [HttpPost("moveToNextStage")]
        public async Task<IActionResult> MoveToNextStage(ManipulateOrderDataInDTO inputData)
        {
            var newHasOrderState = await this._orderService.MoveOrderToNextStage(inputData.OrderId, "Изменено администрацией");

            if(newHasOrderState){
                return Ok(newHasOrderState);
            }
            return BadRequest("Заказ находится на финальной стадии и его состояние не может перейти на следующую стадию");
        }

        [Authorize(Roles = "Admin, KitchenWorker")]
        [HttpPost("moveToPreviousStage")]
        public async Task<IActionResult> MoveToPreviousStage(ManipulateOrderDataInDTO inputData)
        {
            if (await this._orderService.MoveOrderToPreviousStage(inputData.OrderId))
                return Ok();
            return BadRequest("Заказ находится на начальной стадии и его состояние не может перейти на предыдущую стадию");
        }

        [Authorize(Roles = "Client")]
        [HttpPost("reviewOrder")]
        public async Task<IActionResult> ReviewOrder(ReviewOrderInDTO inputData)
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var orderRepo = _repositoryFactory.GetRepository<Order>();

            try
            {
                var reviews = await orderRepo.GetRelationBetweenTwoNodesAsync<ReviewedBy, Client>(new Order() { Id = Guid.Parse(inputData.OrderId) }, new Client() { Id = Guid.Parse(userId) });
                return BadRequest("Вы уже оставили отзыв о данном заказе");
            }
            catch (Exception ex)
            {

            }

            var reviewRelation = new ReviewedBy()
            {
                NodeFromId = Guid.Parse(userId),
                NodeToId = Guid.Parse(inputData.OrderId),
                Review = inputData.Review,
                ClientRating = inputData.ClientRating,
                TimeCreated = DateTime.Now,
            };

            await orderRepo.RelateNodesAsync(reviewRelation);

            return Ok();
        }
    }
}
