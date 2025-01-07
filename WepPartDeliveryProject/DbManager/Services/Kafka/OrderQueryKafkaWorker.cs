using Confluent.Kafka;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics.Metrics;
using System.Text;

namespace DbManager.Services.Kafka
{
    public class OrderQueryKafkaWorker : QueryKafkaWorker
    {
        private readonly ILogger<OrderQueryKafkaWorker> _logger;
        private readonly OrderRepository _orderRepository;
        private readonly Instrumentation _instrumentation;
        private readonly IOrderService _orderService;
        private UpDownCounter<long> _orderCounter { get; }

        public OrderQueryKafkaWorker(IRepositoryFactory repositoryFactory, IOrderService orderService, Instrumentation instrumentation, ILogger<OrderQueryKafkaWorker> logger) : base()
        {
            this._logger = logger;
            this._orderRepository = (OrderRepository)repositoryFactory.GetRepository<Order>();

            this._orderService = orderService;
            this._instrumentation = instrumentation;
            this._orderCounter = instrumentation.Meter.CreateUpDownCounter<long>("order.events", description: "The number of events to save order to database."); ;
        }

        public override void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            base.AddToQueue(consumeResult);
            this._orderCounter.Add(1);
        }

        protected override void WorkFunction()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    ConsumeResult<string, string> consumeResult = null;
                    try
                    {
                        //Если в очереди что-то появилось, то забираем сообщение и парсим его
                        if (!this._queue.TryDequeue(out consumeResult))
                            continue;

                        var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, consumeResult.Message.Headers, (headers, key) => headers.TryGetLastBytes(key, out var headerBytes) ? [Encoding.UTF8.GetString(headerBytes)] : []);
                        //Возможно имеет смысл сделать линком, т.к. все ноды будут читать и обновлять контейнеры
                        using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(ObjectCasheQueryKafkaWorker), System.Diagnostics.ActivityKind.Consumer, parentContext.ActivityContext);

                        var kafkaChangeOrderEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaChangeOrderEvent>(consumeResult.Message.Value);

                        var order = kafkaChangeOrderEvent.Order;

                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Start execute method: {kafkaChangeOrderEvent.MethodName}"));
                        switch (kafkaChangeOrderEvent.MethodName)
                        {
                            case KafkaChangeOrderEvent.AddMethodName:
                                this._orderRepository.AddNodeAsync(order).Wait();
                                break;
                            case KafkaChangeOrderEvent.UpdateMethodName:
                                this._orderRepository.UpdateNodeAsync(order).Wait();
                                break;
                            case KafkaChangeOrderEvent.TryRemoveMethodName:
                                throw new ArgumentException($"KafkaChangeOrderEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed for order");
                            case KafkaChangeOrderEvent.RelateNodesMethodName:
                                this._orderRepository.RelateNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                                break;
                            case KafkaChangeOrderEvent.UpdateRelationNodesMethodName:
                                this._orderRepository.UpdateRelationNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                                break;
                            case KafkaChangeOrderEvent.DeleteRelationNodesMethodName:
                                this._orderRepository.DeleteRelationOfNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                                break;
                            case KafkaChangeOrderEvent.CancelOrderMethodName:
                                {
                                    (string orderId, string reason) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                    this._orderService.CancelOrder(orderId, reason).Wait();
                                    break;
                                }
                            case KafkaChangeOrderEvent.CancelOrderedDishMethodName:
                                {
                                    (string orderId, string dishId) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                    this._orderService.CancelOrderedDish(orderId, dishId).Wait();
                                    break;
                                }
                            case KafkaChangeOrderEvent.ChangeCountOrderedDishMethodName:
                                {
                                    (string orderId, string dishId, int count) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string,int>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                    this._orderService.ChangeCountOrderedDish(orderId, dishId, count).Wait();
                                    break;
                                }
                            case KafkaChangeOrderEvent.MoveOrderToNextStageMethodName:
                                {
                                    (string orderId, string comment) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                    this._orderService.MoveOrderToNextStage(orderId, comment).Wait();
                                    break;
                                }
                            case KafkaChangeOrderEvent.MoveOrderToPreviousStageMethodName:
                                {
                                    string orderId = (string)kafkaChangeOrderEvent.TupleMethodParams;
                                    this._orderService.MoveOrderToPreviousStage(orderId).Wait();
                                    break;
                                }
                            case KafkaChangeOrderEvent.PlaceAnOrderMethodName:
                                {
                                    (string userId, Dictionary<string, int> dishesCounts, string comment, string phoneNumber, string deliveryAddress) 
                                        = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, Dictionary<string, int>, string, string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                    this._orderService.PlaceAnOrder(userId, dishesCounts, comment, phoneNumber, deliveryAddress).Wait();
                                    break;
                                }
                            default:
                                throw new ArgumentException($"KafkaChangeOrderEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed");
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Method {kafkaChangeOrderEvent.MethodName} were executed"));
                        this._orderCounter.Add(-1);

                    }
                    catch (ArgumentException ex)
                    {
                        this._logger.LogError(ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (consumeResult != null)
                        {
                            //в теории возможно ситуация, когда мы добавили объект и тут же его обновили/удалили или обновили и после удалили, однако события были обработаны в другом порядке
                            AddToQueue(consumeResult);
                        }
                        this._logger.LogError(ex.ToString());
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                this._logger.LogError($"{nameof(OrderQueryKafkaWorker)} was interrupted.");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
            }
        }
    }
}
