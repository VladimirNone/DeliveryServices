﻿using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Services;
using Microsoft.Extensions.Options;

namespace WepPartDeliveryProject.BackgroundServices
{
    public class KafkaConsumerBackgroundService : BackgroundService
    {
        private IConsumer<string, string> _consumerBuilder;
        private readonly string _containerEventsTopic;
        private readonly ILogger<KafkaConsumerBackgroundService> _logger;
        private readonly ObjectCasheKafkaChanger _objectCasheKafkaChanger;

        public KafkaConsumerBackgroundService(ObjectCasheKafkaChanger objectCasheKafkaChanger, ILogger<KafkaConsumerBackgroundService> logger, IOptions<KafkaSettings> kafkaOptions)
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
            this._containerEventsTopic = kafkaSettings.ContainerEventsTopic ?? "ContainerEvents";
            this._logger = logger;
            this._objectCasheKafkaChanger = objectCasheKafkaChanger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => DoWork(stoppingToken), stoppingToken);
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                this._logger.LogInformation($"KafkaConsumerBackgroundService subscribe to {this._containerEventsTopic}");
                this._consumerBuilder.Subscribe(this._containerEventsTopic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consume = _consumerBuilder.Consume(cancellationToken);
                        if(consume != null)
                        {
                            if(consume.Topic == this._containerEventsTopic)
                            {
                                this._objectCasheKafkaChanger.AddToQueue(consume);
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