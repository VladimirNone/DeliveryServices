using Confluent.Kafka;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics.Metrics;
using System.Text;

namespace DbManager.Services
{
    public class OrderQueryKafkaWorker : QueryKafkaWorker
    {
        private readonly ILogger<OrderQueryKafkaWorker> _logger;
        private readonly OrderRepository _orderRepository;
        private readonly Instrumentation _instrumentation;
        private UpDownCounter<long> _orderCounter { get; }

        public OrderQueryKafkaWorker(IRepositoryFactory repositoryFactory, Instrumentation instrumentation, ILogger<OrderQueryKafkaWorker> logger) : base()
        {
            this._logger = logger;
            this._orderRepository = (OrderRepository)repositoryFactory.GetRepository<Order>();

            this._instrumentation = instrumentation;
            this._orderCounter = instrumentation.Meter.CreateUpDownCounter<long>("order.events", description: "The number of events to save order to database."); ;
        }

        public override void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            this.AddToQueue(consumeResult);
            this._orderCounter.Add(1);
        }

        protected override void WorkFunction()
        {
            try
            {
                while (!this._cancellationTokenSource.IsCancellationRequested)
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
                        //Получаем тип объекта
                        var objectType = kafkaChangeOrderEvent.TypeObject;

                        if (objectType != typeof(Order))
                        {
                            activity?.AddException(new Exception($"Topic contains non {nameof(Order)} objects"));
                            this._orderCounter.Add(-1);
                            continue;
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("Start catch dublicate Order"));
                        var order = 
                        //TODO: add adding and searching dublicate by key of order
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("Finish catch dublicate Order"));
                        /*---------------------------------------------------------------------------*/
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Start execute method: {kafkaChangeOrderEvent.MethodName}"));
                        switch (kafkaChangeOrderEvent.MethodName)
                        {
                            case KafkaChangeCacheEvent.AddMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                this._orderRepository.AddNodeAsync();
                                break;
                            case KafkaChangeCacheEvent.UpdateMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                objectCacheInfo.UpdateMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Message.Key), node });
                                break;
                            case KafkaChangeCacheEvent.TryRemoveMethodName:
                                throw new ArgumentException($"KafkaChangeCacheEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed for order");
                            default:
                                throw new ArgumentException($"KafkaChangeCacheEvent.MethodName with value \"{kafkaChangeOrderEvent.MethodName}\" can't be processed");
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
                            this.AddToQueue(consumeResult);
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
