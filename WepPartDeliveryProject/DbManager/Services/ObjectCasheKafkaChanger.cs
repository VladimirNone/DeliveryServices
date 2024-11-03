using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Dal;
using DbManager.Data.Cache;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
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
        private KafkaSettings _kafkaSettings;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly DeliveryHealthCheck _deliveryHealthCheck;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private TimeSpan _workInterval = new TimeSpan(0, 0, 1);

        public ObjectCasheKafkaChanger(IRepositoryFactory repositoryFactory, DeliveryHealthCheck deliveryHealthCheck, ILogger<ObjectCasheKafkaChanger> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            this._logger = logger;
            this._kafkaSettings = kafkaOptions.Value;
            this._repositoryFactory = repositoryFactory;
            this._deliveryHealthCheck = deliveryHealthCheck;

            this._workThread = new Thread(this.WorkFunction)
            {
                Name = "ObjectCasheKafkaChanger"
            };
            this._workThread.Start();
        }

        public void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            this.queue.Enqueue(consumeResult);
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
                        if(this.queue.TryDequeue(out consumeResult) && this._deliveryHealthCheck.StartupCompleted)
                        {
                            //Если есть заголовок с именем группы
                            if (consumeResult.Message.Headers.TryGetLastBytes(KafkaConsts.OwnerGroupId, out var groupIdBytes))
                            {
                                //Получаем тип объекта
                                var objectType = Type.GetType(Encoding.UTF8.GetString(consumeResult.Message.Headers.GetLastBytes(KafkaConsts.ObjectType)));

                                ObjectCasheInfo objectCacheInfo;
                                if (!objectCachers.TryGetValue(objectType, out objectCacheInfo))
                                {
                                    objectCacheInfo = new ObjectCasheInfo(); 
                                    objectCacheInfo.Repository = this._repositoryFactory.GetRepository(objectType);

                                    // Создаем тип ObjectCache<T> с заданным T
                                    Type cacheType = typeof(ObjectCache<>).MakeGenericType(objectType);
                                    // Получаем свойство "Instance" для созданного типа
                                    var instanceProperty = cacheType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

                                    objectCacheInfo.InstanceObject = instanceProperty?.GetValue(null);
                                    objectCacheInfo.AddMethod = cacheType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                                    objectCacheInfo.UpdateMethod = cacheType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
                                    objectCacheInfo.TryRemoveMethod = cacheType.GetMethod("TryRemove", BindingFlags.Public | BindingFlags.Instance);

                                    this.objectCachers[objectType] = objectCacheInfo;
                                }

                                var node = objectCacheInfo.Repository.GetNodeAsync(consumeResult.Key, objectType).Result;

                                // Вызвать метод "Add" с необходимыми параметрами
                                objectCacheInfo.AddMethod?.Invoke(objectCacheInfo.InstanceObject, new object[] { Guid.Parse(consumeResult.Key), node });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if(consumeResult != null)
                        {
                            //в теории возможно ситуация, когда мы добавили объект и тут же его обновили/удалили или обновили и после удалили, однако события были обработаны в другом порядке
                            this.queue.Enqueue(consumeResult);
                        }
                        this._logger.LogError(ex.ToString());
                    }

                    this._cancellationTokenSource.Token.WaitHandle.WaitOne(this._workInterval);
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
