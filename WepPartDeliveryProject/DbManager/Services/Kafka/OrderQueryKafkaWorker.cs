using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using WepPartDeliveryProject;

namespace DbManager.Services.Kafka
{
    public class OrderQueryKafkaWorker : QueryKafkaWorker
    {
        private readonly ILogger<OrderQueryKafkaWorker> _logger;
        private readonly OrderRepository _orderRepository;
        private readonly Instrumentation _instrumentation;
        private readonly IOrderService _orderService;
        private UpDownCounter<long> _orderCounter { get; }
        private UpDownCounter<int> _processedOrderCounter { get; }
        private KafkaSettings _kafkaSettings;

        public OrderQueryKafkaWorker(IRepositoryFactory repositoryFactory, IOrderService orderService, Instrumentation instrumentation, ILogger<OrderQueryKafkaWorker> logger, IOptions<KafkaSettings> kafkaSettings) : base()
        {
            this._logger = logger;
            this._orderRepository = (OrderRepository)repositoryFactory.GetRepository<Order>();

            this._orderService = orderService;
            this._instrumentation = instrumentation;
            this._orderCounter = instrumentation.Meter.CreateUpDownCounter<long>("order.events", description: "The number of events to save order to database.");
            this._processedOrderCounter = instrumentation.Meter.CreateUpDownCounter<int>("order.process.events", description: "The number of processed order events.");
            this._kafkaSettings = kafkaSettings.Value;

            this.StartWorker();
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

                    //Если в очереди что-то появилось, то забираем сообщение и парсим его
                    if (!this._queue.TryDequeue(out consumeResult))
                        continue;

                    this._orderCounter.Add(-1);

                    Task.Run(() => ProcessOrderEvent(consumeResult));
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

        private void ProcessOrderEvent(ConsumeResult<string, string> consumeResult)
        {
            try
            {
                //note: render userId via ${scopeproperty:orderId}
                using (this._logger.BeginScope(new[] { new KeyValuePair<string, object>("orderId", consumeResult.Message.Key) }))
                {
                    this._processedOrderCounter.Add(1);

                    var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, consumeResult.Message.Headers, (headers, key) => headers.TryGetLastBytes(key, out var headerBytes) ? [Encoding.UTF8.GetString(headerBytes)] : []);
                    //Возможно имеет смысл сделать линком, т.к. все ноды будут читать и обновлять контейнеры
                    using var activity = this._instrumentation.ActivitySource.StartActivity(nameof(ObjectCasheQueryKafkaWorker), System.Diagnostics.ActivityKind.Consumer, parentContext.ActivityContext);

                    var kafkaChangeOrderEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaChangeOrderEvent>(consumeResult.Message.Value);

                    var order = kafkaChangeOrderEvent.Order;

                    this._logger.LogInformation($"Processing MethodName={kafkaChangeOrderEvent.MethodName}");

                    activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Start execute method: {kafkaChangeOrderEvent.MethodName}"));
                    switch (kafkaChangeOrderEvent.MethodName)
                    {
                        case KafkaChangeOrderEvent.AddMethodName:
                            activity?.AddTag("orderId", order.Id);
                            this._orderRepository.AddNodeAsync(order).Wait();
                            break;
                        case KafkaChangeOrderEvent.UpdateMethodName:
                            activity?.AddTag("orderId", order.Id);
                            this._orderRepository.UpdateNodeAsync(order).Wait();
                            break;
                        case KafkaChangeOrderEvent.TryRemoveMethodName:
                            throw new ArgumentException($"KafkaChangeOrderEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed for order");
                        case KafkaChangeOrderEvent.RelateNodesMethodName:
                            activity?.AddTag("relationId", kafkaChangeOrderEvent.Relation.Id);
                            this._orderRepository.RelateNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                            break;
                        case KafkaChangeOrderEvent.UpdateRelationNodesMethodName:
                            activity?.AddTag("relationId", kafkaChangeOrderEvent.Relation.Id);
                            this._orderRepository.UpdateRelationNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                            break;
                        case KafkaChangeOrderEvent.DeleteRelationNodesMethodName:
                            activity?.AddTag("relationId", kafkaChangeOrderEvent.Relation.Id);
                            this._orderRepository.DeleteRelationOfNodesAsync(kafkaChangeOrderEvent.Relation).Wait();
                            break;
                        case KafkaChangeOrderEvent.CancelOrderMethodName:
                            {
                                (string orderId, string reason) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                activity?.AddTag("orderId", orderId);
                                this._orderService.CancelOrder(orderId, reason).Wait();
                                break;
                            }
                        case KafkaChangeOrderEvent.CancelOrderedDishMethodName:
                            {
                                (string orderId, string dishId) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                activity?.AddTag("orderId", orderId);
                                this._orderService.CancelOrderedDish(orderId, dishId).Wait();
                                break;
                            }
                        case KafkaChangeOrderEvent.ChangeCountOrderedDishMethodName:
                            {
                                (string orderId, string dishId, int count) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string, int>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                activity?.AddTag("orderId", orderId);
                                this._orderService.ChangeCountOrderedDish(orderId, dishId, count).Wait();
                                break;
                            }
                        case KafkaChangeOrderEvent.MoveOrderToNextStageMethodName:
                            {
                                (string orderId, string comment) = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                activity?.AddTag("orderId", orderId);
                                this._orderService.MoveOrderToNextStage(orderId, comment).Wait();
                                break;
                            }
                        case KafkaChangeOrderEvent.MoveOrderToPreviousStageMethodName:
                            {
                                string orderId = (string)kafkaChangeOrderEvent.TupleMethodParams;
                                activity?.AddTag("orderId", orderId);
                                this._orderService.MoveOrderToPreviousStage(orderId).Wait();
                                break;
                            }
                        case KafkaChangeOrderEvent.PlaceAnOrderMethodName:
                            {
                                (string orderId, string userId, Dictionary<string, int> dishesCounts, string comment, string phoneNumber, string deliveryAddress)
                                    = Newtonsoft.Json.JsonConvert.DeserializeObject<ValueTuple<string, string, Dictionary<string, int>, string, string, string>>(((JObject)kafkaChangeOrderEvent.TupleMethodParams).ToString());
                                activity?.AddTag("orderId", orderId);
                                this._orderService.PlaceAnOrder(orderId, userId, dishesCounts, comment, phoneNumber, deliveryAddress).Wait();
                                break;
                            }
                        default:
                            throw new ArgumentException($"KafkaChangeOrderEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed");
                    }
                    activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Method {kafkaChangeOrderEvent.MethodName} were executed"));
                }

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
            finally
            {
                this._processedOrderCounter.Add(-1);
            }
        }
    }
}
