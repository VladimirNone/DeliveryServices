using DbManager.Data.DTOs;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DbManager.Data.Relations;
using AutoMapper;
using DbManager.Neo4j.Implementations;

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

        [HttpPost("placeAnOrder")]
        public async Task<IActionResult> PlaceAnOrder()
        {
            var jsonData = Request.Cookies["cartDishes"];
            var userId = Request.Cookies["X-UserId"];

            var res = jsonData != null ? JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData) : new Dictionary<string, int>();
            if (res.Count == 0)
                return BadRequest("При оформлении заказа, в корзине отсутсвовали продукты");

            var dishes = await _repositoryFactory.GetRepository<Dish>().GetNodesByPropertyAsync("Id", res.Keys.ToArray());

            var order = new Order() { SumWeight = dishes.Sum(h=>h.Weight), Price = dishes.Sum(h => h.Price), DeliveryAddress = "Test Street" };

            var orderRepo = _repositoryFactory.GetRepository<Order>();

            await orderRepo.AddNodeAsync(order);
            await orderRepo.RelateNodesAsync(new Ordered() { NodeFromId = Guid.Parse(userId), NodeTo = order });

            foreach (var dish in dishes)
            {
                await orderRepo.RelateNodesAsync(new OrderedDish() { NodeFrom = order, NodeTo = dish, Count = res[dish.Id.ToString()] });
            }

            var kitchens = await _repositoryFactory.GetRepository<Kitchen>().GetNodesAsync();
            var randomKitchen = kitchens[new Random().Next(0, kitchens.Count)];

            await orderRepo.RelateNodesAsync(new CookedBy() { NodeFrom = randomKitchen, NodeTo = order });

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
        public async Task<IActionResult> GetClientOrders(int page = 0)
        {
            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var ordereds = await _repositoryFactory.GetRepository<Client>().GetRelationsOfNodesAsync<Ordered, Order>(userId);
            var orders = ordereds.Select(h => _mapper.Map<OrderOutDTO>((Order)h.NodeTo)).ToList();

            return Ok(orders);
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
                var orderedDishes = await _repositoryFactory.GetRepository<Order>().GetRelationsOfNodesAsync<OrderedDish, Dish>(searchedOrder);

                var preparedOrder = _mapper.Map<OrderOutDTO>(searchedOrder);
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

        [Authorize(Roles = "Admin")]
        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder(ManipulateOrderDataInDTO inputData)
        {
            var order = await _repositoryFactory.GetRepository<Order>().GetNodeAsync(inputData.OrderId);

            await _repositoryFactory.GetRepository<Order>().DeleteNodeWithAllRelations(order);

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
    }
}
