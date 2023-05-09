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
            new StatisticQueryInfoOutDTO(){ NameQuery = "После какой стадии чаще всего отменяют заказ", LinkToQuery = "query4", ChartName = "line", NeedDataRange = true},
            new StatisticQueryInfoOutDTO(){ NameQuery = "Сколько в среднем клиенты оформляют заказов", LinkToQuery = "query5", ChartName = "line", NeedDataRange = true},
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
    }
}
