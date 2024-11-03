using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;

namespace DbManager.Services
{
    public class ObjectCasheKafkaChanger : IDisposable
    {
        private ConcurrentQueue<ConsumeResult<string, string>> queue = new ConcurrentQueue<ConsumeResult<string, string>>();
        private Thread _workThread;
        private ILogger<ObjectCasheKafkaChanger> _logger;
        private KafkaSettings _kafkaSettings;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private TimeSpan _workInterval = new TimeSpan(0, 0, 1);

        public ObjectCasheKafkaChanger(RepositoryFactory repositoryFactory, ILogger<ObjectCasheKafkaChanger> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            this._logger = logger;
            this._kafkaSettings = kafkaOptions.Value;
            this._repositoryFactory = repositoryFactory;

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
                    try
                    {
                        //Если в очереди что-то появилось, то забираем сообщение и парсим его
                        if(this.queue.TryDequeue(out var consumeResult))
                        {
                            //Если есть заголовок с именем группы
                            if (consumeResult.Message.Headers.TryGetLastBytes(KafkaConsts.OwnerGroupId, out var groupIdBytes))
                            {
                                //Если это наше событие, то скипаем его, т.к. мы его уже записали сами в бд
                                if (Encoding.UTF8.GetString(groupIdBytes) == this._kafkaSettings.GroupId)
                                    continue;
                                //Получаем тип объекта
                                var objectType = Type.GetType(Encoding.UTF8.GetString(consumeResult.Message.Headers.GetLastBytes(KafkaConsts.ObjectType)));

                                //this._repositoryFactory.GetRepository()
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex.ToString());
                    }

                    this._cancellationTokenSource.Token.WaitHandle.WaitOne(this._workInterval);
                }
            }
            catch (ThreadInterruptedException)
            {
                this._logger.LogError("ObjectCasheKafkaChanger was interrupted.");
            }
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
