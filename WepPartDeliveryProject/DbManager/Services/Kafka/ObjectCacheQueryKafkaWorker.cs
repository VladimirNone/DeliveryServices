using Confluent.Kafka;
using DbManager.Dal;
using DbManager.Data.Cache;
using DbManager.Data.Kafka;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text;

namespace DbManager.Services.Kafka
{
    public class ObjectCacheQueryKafkaWorker : QueryKafkaWorker
    {
        private readonly ConcurrentDictionary<Type, ObjectCasheInfo> objectCachers = new ConcurrentDictionary<Type, ObjectCasheInfo>();
        private readonly ILogger<ObjectCacheQueryKafkaWorker> _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly Instrumentation _instrumentation;
        private UpDownCounter<long> _cacheEventCounter { get; }

        public ObjectCacheQueryKafkaWorker(IRepositoryFactory repositoryFactory, Instrumentation instrumentation, ILogger<ObjectCacheQueryKafkaWorker> logger) : base()
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;

            _instrumentation = instrumentation;
            _cacheEventCounter = instrumentation.Meter.CreateUpDownCounter<long>("cache.events", description: "The number of events for changing object cache on cluster.");

            this.StartWorker();
        }

        public override void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            base.AddToQueue(consumeResult);
            _cacheEventCounter.Add(1);
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
                        if (!_queue.TryDequeue(out consumeResult))
                            continue;

                        var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, consumeResult.Message.Headers, (headers, key) => headers.TryGetLastBytes(key, out var headerBytes) ? [Encoding.UTF8.GetString(headerBytes)] : []);
                        //Возможно имеет смысл сделать линком, т.к. все ноды будут читать и обновлять контейнеры
                        using var activity = _instrumentation.ActivitySource.StartActivity(nameof(ObjectCacheQueryKafkaWorker), System.Diagnostics.ActivityKind.Consumer, parentContext.ActivityContext);

                        var kafkaChangeCacheEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaChangeCacheEvent>(consumeResult.Message.Value);
                        //Получаем тип объекта
                        var objectType = kafkaChangeCacheEvent.TypeObject;

                        ObjectCasheInfo objectCacheInfo;
                        if (!objectCachers.TryGetValue(objectType, out objectCacheInfo))
                        {
                            activity?.AddEvent(new System.Diagnostics.ActivityEvent("Start CreatingCachInfo"));
                            objectCacheInfo = new ObjectCasheInfo();
                            objectCacheInfo.Repository = _repositoryFactory.GetRepository(objectType);

                            // Создаем тип ObjectCache<T> с заданным T
                            Type cacheType = typeof(ObjectCache<>).MakeGenericType(objectType);
                            // Получаем свойство "Instance" для созданного типа
                            var instanceProperty = cacheType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

                            objectCacheInfo.InstanceObject = instanceProperty?.GetValue(null);
                            objectCacheInfo.AddMethod = cacheType.GetMethod(KafkaChangeCacheEvent.AddMethodName, BindingFlags.Public | BindingFlags.Instance);
                            objectCacheInfo.UpdateMethod = cacheType.GetMethod(KafkaChangeCacheEvent.UpdateMethodName, BindingFlags.Public | BindingFlags.Instance);
                            objectCacheInfo.TryRemoveMethod = cacheType.GetMethod(KafkaChangeCacheEvent.TryRemoveMethodName, BindingFlags.Public | BindingFlags.Instance);

                            objectCachers[objectType] = objectCacheInfo;
                            activity?.AddEvent(new System.Diagnostics.ActivityEvent("Finish CreatingCachInfo"));
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("Start GetNodeAsync"));
                        var node = objectCacheInfo.Repository.GetNodeAsync(consumeResult.Message.Key, objectType).Result;
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Finish GetNodeAsync and start execute method: {kafkaChangeCacheEvent.MethodName}"));
                        switch (kafkaChangeCacheEvent.MethodName)
                        {
                            case KafkaChangeCacheEvent.AddMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                objectCacheInfo.AddMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Message.Key), node });
                                break;
                            case KafkaChangeCacheEvent.UpdateMethodName:
                                // Вызываем метод "Update" с необходимыми параметрами
                                objectCacheInfo.UpdateMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Message.Key), node });
                                break;
                            case KafkaChangeCacheEvent.TryRemoveMethodName:
                                // Вызываем метод "TryRemove" с необходимыми параметрами
                                objectCacheInfo.TryRemoveMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Message.Key) });
                                break;
                            default:
                                throw new ArgumentException($"KafkaChangeCacheEvent.MethodName with value \"{kafkaChangeCacheEvent.MethodName}\" can't be processed");
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Method {kafkaChangeCacheEvent.MethodName} were executed"));
                        _cacheEventCounter.Add(-1);

                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (consumeResult != null)
                        {
                            //в теории возможно ситуация, когда мы добавили объект и тут же его обновили/удалили или обновили и после удалили, однако события были обработаны в другом порядке
                            AddToQueue(consumeResult);
                        }
                        _logger.LogError(ex.ToString());
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                _logger.LogError($"{nameof(ObjectCacheQueryKafkaWorker)} was interrupted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private class ObjectCasheInfo
        {
            public object? InstanceObject { get; set; }
            public MethodInfo? AddMethod { get; set; }
            public MethodInfo? UpdateMethod { get; set; }
            public MethodInfo? TryRemoveMethod { get; set; }
            public IGeneralRepository Repository { get; set; }
        }
    }
}
