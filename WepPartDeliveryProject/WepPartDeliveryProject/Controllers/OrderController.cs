using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DbManager.Data.Relations;
using AutoMapper;
using DbManager.Data;
using System.Xml.Linq;

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

        public OrderController(IRepositoryFactory repositoryFactory, IConfiguration configuration, IMapper mapper, JwtService jwtService)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var jsonData = Request.Cookies["cartDishes"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", res.Keys.ToArray());

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

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", res.Keys.ToArray());

            var order = new Order() { 
                SumWeight = dishes.Sum(h=>h.Weight), 
                Price = dishes.Sum(h => h.Price), 
                DeliveryAddress = inputData.DeliveryAddress, 
                PhoneNumber = inputData.PhoneNumber };

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

            await orderRepo.AddNodeAsync(order);

/*            var kitchens = await _repositoryFactory.GetRepository<Kitchen>().GetNodesAsync();
            var randomKitchen = kitchens[new Random().Next(0, kitchens.Count)];*/
            var randomKitchen = await _repositoryFactory.GetRepository<Kitchen>().GetNodeAsync("82e5e232-1987-4dcd-bd3a-8841e3f7707b");


/*            var deliveryMen = await _repositoryFactory.GetRepository<DeliveryMan>().GetNodesAsync();
            var randomdelMan = deliveryMen[new Random().Next(0, deliveryMen.Count)];*/
            var randomdelMan = await _repositoryFactory.GetRepository<DeliveryMan>().GetNodeAsync("7ac11adb-3631-4e01-af74-eeeb7287a034");

            await orderRepo.CreateOrderRelationInDB(order, userId, dishes, randomKitchen, randomdelMan, res, inputData.Comment);

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("changeCountOrderedDish")]
        public async Task<IActionResult> ChangeCountOrderedDish(ManipulateOrderDataInDTO inputData)
        {
            var order = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(inputData.OrderId);

            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.DishId);

            var orderedDish = await _repositoryFactory.GetRepository<Dish>().GetRelationBetweenTwoNodesAsync<OrderedDish, Order>(dish, order);

            order.Price -= orderedDish.Count * dish.Price;

            orderedDish.Count = (int)inputData.NewCount;

            order.Price += orderedDish.Count * dish.Price;

            await _repositoryFactory.GetRepository<Order>().UpdateNodeAsync(order);

            await _repositoryFactory.GetRepository<Order>().UpdateRelationNodesAsync(orderedDish);

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

            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);

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

                return Ok(new { order = preparedOrder, orderedDishes = orderedDishes.Select(h=> new {count = h.Count, dishInfo = h.NodeTo}) });
            }

            return BadRequest("Запрашиваемый заказ не доступен данному пользователю или не существует");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("cancelOrderedDish")]
        public async Task<IActionResult> CancelOrderedDish(ManipulateOrderDataInDTO inputData)
        {
            var order = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(inputData.OrderId);

            var dish = await _repositoryFactory.GetRepository<Dish>().GetNodeAsync(inputData.DishId);

            var orderedDish = await _repositoryFactory.GetRepository<Dish>().GetRelationBetweenTwoNodesAsync<OrderedDish, Order>(dish, order);

            order.Price -= orderedDish.Count * dish.Price;

            await _repositoryFactory.GetRepository<Order>().UpdateNodeAsync(order);

            await _repositoryFactory.GetRepository<Order>().DeleteRelationOfNodesAsync<OrderedDish, Dish>(order, dish);

            return Ok();
        }

        [Authorize(Roles = "Admin, Client")]
        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder(ManipulateOrderDataInDTO inputData)
        {
            var orderRepo = _repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(inputData.OrderId);
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);

            await orderRepo.DeleteRelationOfNodesAsync<HasOrderState, OrderState>(order, orderState);

            var cancelState = OrderState.OrderStatesFromDb.First(h => h.NumberOfStage == (int)OrderStateEnum.Cancelled);
            var relationCancel = new HasOrderState() { Comment = inputData.ReasonOfCancel, NodeFromId = order.Id, NodeToId = cancelState.Id, TimeStartState = DateTime.Now };

            order.Story.Add(relationCancel);
            await orderRepo.RelateNodesAsync(relationCancel);
            await orderRepo.UpdateNodeAsync(order);

            return Ok();
        }

        [Authorize(Roles = "Admin, KitchenWorker, DeliveryMan")]
        [HttpPost("moveToNextStage")]
        public async Task<IActionResult> MoveToNextStage(ManipulateOrderDataInDTO inputData)
        {
            var orderRepo = (IOrderRepository) _repositoryFactory.GetRepository<Order>(true);
            var newHasOrderState = await orderRepo.MoveOrderToNextStage(inputData.OrderId, "Изменено администрацией");

            if(newHasOrderState != null){
                return Ok(_mapper.Map<OrderStateItemOutDTO>(newHasOrderState));
            }
            return BadRequest("Заказ находится на финальной стадии и его состояние не может перейти на следующую стадию");
        }

        [Authorize(Roles = "Admin, KitchenWorker")]
        [HttpPost("moveToPreviousStage")]
        public async Task<IActionResult> MoveToPreviousStage(ManipulateOrderDataInDTO inputData)
        {
            var orderRepo = (IOrderRepository)_repositoryFactory.GetRepository<Order>(true);
            if (await orderRepo.MoveOrderToPreviousStage(inputData.OrderId))
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
