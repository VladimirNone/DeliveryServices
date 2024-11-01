using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace WepPartDeliveryProject.BackgroundServices
{
    public class KafkaConsumerBackgroundService : BackgroundService
    {
        private IConsumer<Ignore, string> _consumerBuilder;
        private readonly string _topic;
        private readonly ILogger<KafkaConsumerBackgroundService> _logger;

        public KafkaConsumerBackgroundService(IConfiguration configuration, ILogger<KafkaConsumerBackgroundService> logger)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["BootstrapServers"],
                GroupId = "1",
                AutoOffsetReset = AutoOffsetReset.Latest,
                Acks = Acks.All,
            };
            this._consumerBuilder = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            this._topic = configuration["ContainerEventsTopic"] ?? "ContainerEvents";
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => DoWork(stoppingToken), stoppingToken);
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                this._logger.LogInformation($"KafkaConsumerBackgroundService subscribe to {this._topic}");
                this._consumerBuilder.Subscribe(this._topic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumer = _consumerBuilder.Consume(cancellationToken);
                        Console.WriteLine($"Processing Employee Name: {consumer.Message.Value}");

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
