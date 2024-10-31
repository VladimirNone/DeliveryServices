using Confluent.Kafka;

namespace WepPartDeliveryProject.BackgroundServices
{
    public class KafkaConsumerBackgroundService : BackgroundService
    {
        private IConsumer<Ignore, string> _consumerBuilder { get; set; }
        private readonly string _topic;

        public KafkaConsumerBackgroundService(IConfiguration configuration)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["BootstrapServers"],
                GroupId = "1",
                AutoOffsetReset = AutoOffsetReset.Latest,
                Acks = Acks.All,
            };
            _consumerBuilder = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            _topic = configuration["ContainerEventsTopic"] ?? "ContainerEvents";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => DoWork(stoppingToken), stoppingToken);
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                _consumerBuilder.Subscribe(_topic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumer = _consumerBuilder.Consume(cancellationToken);
                        Console.WriteLine($"Processing Employee Name: {consumer.Message.Value}");

                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        // Consumer errors should generally be ignored (or logged) unless fatal.
                        Console.WriteLine($"Consume error: {e.Error.Reason}");

                        if (e.Error.IsFatal)
                        {
                            // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
