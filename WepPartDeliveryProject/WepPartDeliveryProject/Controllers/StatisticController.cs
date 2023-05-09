using DbManager.Data.DTOs;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbManager.Data.Nodes;
using AutoMapper;
using DbManager.Data.Relations;

namespace WepPartDeliveryProject.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class StatisticController : Controller
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ApplicationSettings _appSettings;
        private readonly IMapper _mapper;

        public StatisticController(IRepositoryFactory repositoryFactory, IConfiguration configuration, IMapper mapper)
        {
            // Fetch settings object from configuration
            _appSettings = new ApplicationSettings();
            configuration.GetSection("ApplicationSettings").Bind(_appSettings);

            _repositoryFactory = repositoryFactory;
            _mapper = mapper;
        }

        public static List<StatisticQueryInfoOutDTO> Statistics { get; } = new List<StatisticQueryInfoOutDTO>()
        {
            new StatisticQueryInfoOutDTO(){ NameQuery = "Количество и суммарная стоимость заказов по месяцам", LinkToQuery = "query1", ChartName = "line", NeedDataRange = true, NameDatasets = new List<string>(){ "Стоимость заказов", "Количество заказов" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Топ-10 клиентов по стоимости заказов", LinkToQuery = "query2", ChartName = "line", NeedDataRange = true, NameDatasets = new List<string>(){ "Стоимость заказов", "Количество заказов" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Топ доставщиков по количеству заказов", LinkToQuery = "query3", ChartName = "line", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Количество отмененных заказов на каждой стадии", LinkToQuery = "query4", ChartName = "radar", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Сколько в среднем клиенты оформляют заказов, если оформляют", LinkToQuery = "query5", ChartName = "bar", NeedDataRange = true, NameDatasets = new List<string>(){ "Количество заказов", "Количество клиентов, оформивших заказы" } },
            new StatisticQueryInfoOutDTO(){ NameQuery = "Средняя продолжительность пребывания заказа в стадии", LinkToQuery = "query6", ChartName = "line", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Сравнение показателей по месяцам", LinkToQuery = "query7", ChartName = "radar", NeedDataRange = true},
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

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>(true)).GetOrderPriceAndCountStatistic();

            foreach (var item in resData)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1 });
            }

/*            var finishedOrderState = OrderState.OrderStatesFromDb.First(h => h.NumberOfStage == (int)OrderStateEnum.Finished);
            var finishedRelationsOrder = await _repositoryFactory
                .GetRepository<OrderState>()
                .GetRelationsOfNodesAsync<HasOrderState, Order>(finishedOrderState);

            finishedRelationsOrder.Sort((x,y) => x.TimeStartState.CompareTo(y.TimeStartState));

            foreach (var item in finishedRelationsOrder)
            {
                var xName = item.TimeStartState.Month + "." + item.TimeStartState.Year;
                var foundedQueryItem = resQueryData.Find(h => h.X == xName);
                if (foundedQueryItem != null)
                {
                    foundedQueryItem.Y[0] += ((Order)item.NodeFrom).Price;
                    foundedQueryItem.Y[1]++;
                }
                else
                {
                    resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { ((Order)item.NodeFrom).Price, 1 }, X = xName });
                }
            }*/

            return Ok(resQueryData);
        }

        [HttpGet("query2")]
        public async Task<IActionResult> GetQuery2()
        {
            var resQueryData = new List<StatisticQueryDataItemOutDTO>();

            var clientRepo = (IClientRepository)_repositoryFactory.GetRepository<Client>(true);

            var topClients = await clientRepo.GetTopClientBySumPriceOrder(10);
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

            var delManRepo = (IDeliveryManRepository)_repositoryFactory.GetRepository<DeliveryMan>(true);

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

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>(true)).GetCancelledOrderGroupedByMonthStatistic();

            var nameDatasets = new List<string>();

            for (int i = 0; i < resData.Count; i++)
            {
                nameDatasets.Add(resData[i].Item1);

                foreach (var order in resData[i].Item2)
                {
                    var previousHasOrderState = order.Story[order.Story.Count - 2];
                    previousHasOrderState.NodeTo = OrderState.OrderStatesFromDb.Find(el => el.Id == previousHasOrderState.NodeToId);
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

            var resData = await ((IOrderRepository)_repositoryFactory.GetRepository<Order>(true)).GetCountFinishedOrderAndClientsStatistic();

            foreach (var item in resData)
            {
                resQueryData.Add(new StatisticQueryDataItemOutDTO() { Y = new List<double>() { item.Item2, item.Item3 }, X = item.Item1 });
            }

            return Ok(resQueryData);
        }
    }
}
