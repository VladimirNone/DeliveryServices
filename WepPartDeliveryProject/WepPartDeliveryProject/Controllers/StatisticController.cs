using DbManager.Data.DTOs;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbManager.Data.Nodes;
using AutoMapper;
using DbManager.Data.Relations;
using Microsoft.Extensions.Options;
using DbManager.Data.Cache;

namespace WepPartDeliveryProject.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class StatisticController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;

        public StatisticController(IRepositoryFactory repositoryFactory, IMapper mapper, IOptions<ApplicationSettings> appSettingsOptions)
        {
            // Fetch settings object from configuration
            _appSettings = appSettingsOptions.Value;

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        public static List<StatisticQueryInfoOutDTO> Statistics { get; } = new List<StatisticQueryInfoOutDTO>()
        {
            new StatisticQueryInfoOutDTO(){ NameQuery = "Количество и суммарная стоимость заказов по месяцам",                      LinkToQuery = "query1", ChartName = "line", NeedDataRange = true, NameDatasets = new List<string>(){ "Стоимость заказов", "Количество заказов" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Топ-10 клиентов по стоимости заказов",                                     LinkToQuery = "query2", ChartName = "line", NeedDataRange = true, NameDatasets = new List<string>(){ "Стоимость заказов", "Количество заказов" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Топ доставщиков по количеству заказов",                                    LinkToQuery = "query3", ChartName = "line", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Количество отмененных заказов на каждой стадии",                           LinkToQuery = "query4", ChartName = "radar",NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Сколько в среднем клиенты оформляют заказов, если оформляют",              LinkToQuery = "query5", ChartName = "bar",  NeedDataRange = true, NameDatasets = new List<string>(){ "Количество заказов", "Количество клиентов, оформивших заказы" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Средняя продолжительность пребывания заказа в стадии",                     LinkToQuery = "query6", ChartName = "line", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Количество выполненных заказов и приготовленных блюд в каждой из кухонь",  LinkToQuery = "query7", ChartName = "bar",  NeedDataRange = true, NameDatasets = new List<string>(){ "Количество выполненных заказов", "Количество приготовленных блюд" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Топ-10 самых популярных блюд",                                             LinkToQuery = "query8", ChartName = "line", NeedDataRange = true},
        };

        [AllowAnonymous]
        [HttpGet("getStatisticQueries")]
        public IActionResult GetStatisticQueries()
        {
            return Ok(Statistics);
        }

        [HttpGet("query1")]
        public async Task<IActionResult> GetQuery1()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>()).GetOrderPriceAndCountStatistic();

            foreach (var item in resData)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1 });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query2")]
        public async Task<IActionResult> GetQuery2()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var clientRepo = (IClientRepository)_repositoryFactory.GetRepository<Client>();

            var topClients = await clientRepo.GetTopClientBySumPriceOrderStatistic(10);
            topClients.Reverse();

            foreach (var item in topClients)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1.Login });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query3")]
        public async Task<IActionResult> GetQuery3()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var delManRepo = (IDeliveryManRepository)_repositoryFactory.GetRepository<DeliveryMan>();

            var topDelMen = await delManRepo.GetTopDeliveryMenByCountOrder(10);
            topDelMen.Reverse();

            foreach (var item in topDelMen)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2 }, X = item.Item1.Name });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query4")]
        public async Task<IActionResult> GetQuery4()
        {
            //месяц только для диапазона, выводиться будет как название dataset
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>()).GetCancelledOrderGroupedByMonthStatistic();

            var nameDatasets = new List<string>();

            for (int i = 0; i < resData.Count; i++)
            {
                nameDatasets.Add(resData[i].Item1);

                foreach (var order in resData[i].Item2)
                {
                    var previousHasOrderState = order.Story[order.Story.Count - 2];
                    previousHasOrderState.NodeTo = ObjectCache<OrderState>.Instance.ToList().Find(el => el.Id == previousHasOrderState.NodeToId);
                    var previousNameOfState = ((OrderState)previousHasOrderState.NodeTo).NameOfState;
                    var dataItem = resQueryData.Find(el => el.X == previousNameOfState);

                    if(dataItem == null)
                    {
                        dataItem = new StatisticQueryDataItemOutDTO() { Y = new List<double>(), X = previousNameOfState };
                        resQueryData.Add(dataItem);
                    }

                    foreach (var item in resQueryData)
                    {
                        while (item.Y.Count != i + 1)
                            item.Y.Add(0);
                    }

                    dataItem.Y[i]++;
                }

                
            }

            return Ok(new { queryData = resQueryData, nameDatasets });
        }

        [HttpGet("query5")]
        public async Task<IActionResult> GetQuery5()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>()).GetCountFinishedOrderAndClientsStatistic();

            foreach (var item in resData)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1 });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query6")]
        public async Task<IActionResult> GetQuery6()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>()).GetNodesAsync(limitCount:100);

            for (int i = 0, j = 1; i < ObjectCache<OrderState>.Instance.Count(); i++, j *= 2)
            {
                if (j == (int)OrderStateEnum.Cancelled || j == (int)OrderStateEnum.Finished)
                {
                    break;
                }
                var curState = ObjectCache<OrderState>.Instance.First(h => h.NumberOfStage == j);

                var orderMinutesBetweenCurStateAndNext = new List<int>();
                var needRemoveOrders = new List<Order>();
                foreach (var order in resData)
                {
                    //количество состояний в истории должно быть на одну больше чем нужно, чтобы было ОТ и ДО
                    if(order.Story.Count > i + 1)
                    {
                        orderMinutesBetweenCurStateAndNext.Add((order.Story[i+1].TimeStartState - order.Story[i].TimeStartState).Minutes);
                    }
                    else
                    {
                        //удаляем потому что нам известно, что эти заказы закончились на текущей стадии
                        needRemoveOrders.Add(order);
                    }
                }

                resData = resData.Except(needRemoveOrders).ToList();

                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { orderMinutesBetweenCurStateAndNext.Sum()/ orderMinutesBetweenCurStateAndNext.Count }, X = curState.NameOfState });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query7")]
        public async Task<IActionResult> GetQuery7()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>()).GetCountOrdersAndOrderedDishesForEveryKitchenStatistic();

            foreach (var item in resData)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1.Address });
            }

            return Ok(resQueryData);
        }

        [HttpGet("query8")]
        public async Task<IActionResult> GetQuery8()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var dishRepo = (IDishRepository)_repositoryFactory.GetRepository<Dish>();

            var topDish = await dishRepo.GetTopDishByCountOrderedStatistic(10);
            topDish.Reverse();

            foreach (var item in topDish)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2 }, X = item.Item1.Name });
            }

            return Ok(resQueryData);
        }
    }
}
