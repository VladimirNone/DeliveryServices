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

namespace DbManager.Services
{
    public class ObjectCasheKafkaChanger : IDisposable
    {
        private ConcurrentQueue<ConsumeResult<string, string>> queue = new ConcurrentQueue<ConsumeResult<string, string>>();
        private ConcurrentDictionary<Type, ObjectCasheInfo> objectCachers = new ConcurrentDictionary<Type, ObjectCasheInfo>();
        private Thread _workThread;
        private ILogger<ObjectCasheKafkaChanger> _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly Counter<long> _cacheEventCounter;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Instrumentation _instrumentation;

        public ObjectCasheKafkaChanger(IRepositoryFactory repositoryFactory, Instrumentation instrumentation, ILogger<ObjectCasheKafkaChanger> logger)
        {
            this._logger = logger;
            this._repositoryFactory = repositoryFactory;

            this._cacheEventCounter = instrumentation.CacheEventCounter;

            this._instrumentation = instrumentation;

            this._workThread = new Thread(this.WorkFunction) { Name = "ObjectCasheKafkaChanger" };
            this._workThread.Start();
        }

        public void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            this.queue.Enqueue(consumeResult);
            this._cacheEventCounter.Add(1);
        }

        private void WorkFunction()
        {
            try
            {
                while (!this._cancellationTokenSource.IsCancellationRequested)
                {
                    ConsumeResult<string, string> consumeResult = null;
                    try
                    {
                        //Если в очереди что-то появилось и при этом сервис разогрелся, то забираем сообщение и парсим его
                        if (!this.queue.TryDequeue(out consumeResult))
                            continue;
                        
                        var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, consumeResult.Message.Headers, (headers, key) => headers.TryGetLastBytes(key, out var headerBytes) ? [Encoding.UTF8.GetString(headerBytes)] : []);
                        //Возможно имеет смысл сделать линком, т.к. все ноды будут читать и обновлять контейнеры
                        using var activity = this._instrumentation.ActivitySource.StartActivity("ObjectCasheKafkaChanger", System.Diagnostics.ActivityKind.Consumer, parentContext.ActivityContext);

                        var kafkaChangeCacheEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaChangeCacheEvent>(consumeResult.Message.Value);
                        //Получаем тип объекта
                        var objectType = kafkaChangeCacheEvent.TypeCacheObject;

                        ObjectCasheInfo objectCacheInfo;
                        if (!objectCachers.TryGetValue(objectType, out objectCacheInfo))
                        {
                            activity?.AddEvent(new System.Diagnostics.ActivityEvent("Start CreatingCachInfo"));
                            objectCacheInfo = new ObjectCasheInfo();
                            objectCacheInfo.Repository = this._repositoryFactory.GetRepository(objectType);

                            // Создаем тип ObjectCache<T> с заданным T
                            Type cacheType = typeof(ObjectCache<>).MakeGenericType(objectType);
                            // Получаем свойство "Instance" для созданного типа
                            var instanceProperty = cacheType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

                            objectCacheInfo.InstanceObject = instanceProperty?.GetValue(null);
                            objectCacheInfo.AddMethod = cacheType.GetMethod(KafkaChangeCacheEvent.AddMethodName, BindingFlags.Public | BindingFlags.Instance);
                            objectCacheInfo.UpdateMethod = cacheType.GetMethod(KafkaChangeCacheEvent.UpdateMethodName, BindingFlags.Public | BindingFlags.Instance);
                            objectCacheInfo.TryRemoveMethod = cacheType.GetMethod(KafkaChangeCacheEvent.TryRemoveMethodName, BindingFlags.Public | BindingFlags.Instance);

                            this.objectCachers[objectType] = objectCacheInfo;
                            activity?.AddEvent(new System.Diagnostics.ActivityEvent("Finish CreatingCachInfo"));
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent("Start GetNodeAsync"));
                        var node = objectCacheInfo.Repository.GetNodeAsync(consumeResult.Key, objectType).Result;
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Finish GetNodeAsync and start execute method: {kafkaChangeCacheEvent.MethodName}"));
                        switch (kafkaChangeCacheEvent.MethodName)
                        {
                            case KafkaChangeCacheEvent.AddMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                objectCacheInfo.AddMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Key), node });
                                break;
                            case KafkaChangeCacheEvent.UpdateMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                objectCacheInfo.UpdateMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Key), node });
                                break;
                            case KafkaChangeCacheEvent.TryRemoveMethodName:
                                // Вызываем метод "Add" с необходимыми параметрами
                                objectCacheInfo.TryRemoveMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Key) });
                                break;
                            default:
                                throw new ArgumentException($"KafkaChangeCacheEvent.MethodName with value \"{kafkaChangeCacheEvent.MethodName}\" can't be processed");
                        }
                        activity?.AddEvent(new System.Diagnostics.ActivityEvent($"Method {kafkaChangeCacheEvent.MethodName} were executed"));
                        this._cacheEventCounter.Add(-1);
                        
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
                            this.queue.Enqueue(consumeResult);
                        }
                        this._logger.LogError(ex.ToString());
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                this._logger.LogError("ObjectCasheKafkaChanger was interrupted.");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
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

        public void Dispose()
        {
            if (this._workThread != null)
            {
                this._cancellationTokenSource.Cancel();

                this._workThread.Interrupt();
                this._workThread.Join();
                this._workThread = null;

                this._cancellationTokenSource.Dispose();
            }
        }
    }
}
