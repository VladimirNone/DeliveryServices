using Confluent.Kafka;
using DbManager.AppSettings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbManager.Services
{
    public class KafkaConsumerBackgroundService : BackgroundService
    {
        private IConsumer<string, string> _consumerBuilder;
        private readonly string _containerTopic;
        private readonly ILogger<KafkaConsumerBackgroundService> _logger;
        private readonly DeliveryHealthCheck _deliveryHealthCheck;
        private readonly QueryKafkaWorker _queryKafkaWorker;

        public KafkaConsumerBackgroundService(QueryKafkaWorker queryKafkaWorker, DeliveryHealthCheck deliveryHealthCheck, ILogger<KafkaConsumerBackgroundService> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            var kafkaSettings = kafkaOptions.Value;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
                Acks = Acks.All,
            };
            this._consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig).Build();
            this._containerTopic = queryKafkaWorker is OrderQueryKafkaWorker ? kafkaSettings.ContainerOrdersTopic : kafkaSettings.ContainerEventsTopic;
            this._logger = logger;
            this._queryKafkaWorker = queryKafkaWorker;
            this._deliveryHealthCheck = deliveryHealthCheck;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => DoWork(stoppingToken), stoppingToken);
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                //не подписываемся на топик пока не разогреется текущий сервис
                while (!this._deliveryHealthCheck.StartupCompleted) 
                {
                    cancellationToken.WaitHandle.WaitOne(200);
                }
                this._logger.LogInformation($"KafkaConsumerBackgroundService subscribe to {this._containerTopic}");
                this._consumerBuilder.Subscribe(this._containerTopic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consume = _consumerBuilder.Consume(cancellationToken);
                        if(consume != null)
                        {
                            if(consume.Topic == this._containerTopic)
                            {
                                this._queryKafkaWorker.AddToQueue(consume);
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        this._logger.LogCritical(ex.ToString());
                        throw;
                    }
                    catch (ConsumeException e)
                    {
                        // Consumer errors should generally be ignored (or logged) unless fatal.
                        this._logger.LogCritical($"Consume error: {e.Error.Reason}. Stacktrace: {e}");

                        if (e.Error.IsFatal)
                        {
                            // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex.ToString());
                throw;
            }
        }

        public override void Dispose()
        {
            _consumerBuilder.Close();
            _consumerBuilder.Dispose();

            base.Dispose();
        }
    }
}
