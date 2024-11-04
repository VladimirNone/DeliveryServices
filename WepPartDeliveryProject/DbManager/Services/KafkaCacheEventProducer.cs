using DbManager.AppSettings;
using DbManager.Data;
using DbManager.Data.Cache;
using DbManager.Data.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DbManager.Services
{
    public class KafkaCacheEventProducer
    {
        private readonly KafkaDependentProducer<string, string> _kafkaProducer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly string _topic;
        private readonly DeliveryHealthCheck _deliveryHealthCheck;
        private readonly ILogger<KafkaCacheEventProducer> _logger;

        public KafkaCacheEventProducer(KafkaDependentProducer<string, string> kafkaProducer, IOptions<KafkaSettings> kafkaOptions, DeliveryHealthCheck deliveryHealthCheck, ILogger<KafkaCacheEventProducer> logger)
        {
            this._kafkaSettings = kafkaOptions.Value;
            this._topic = this._kafkaSettings.ContainerEventsTopic ?? "ContainerEvents";
            this._kafkaProducer = kafkaProducer;
            this._deliveryHealthCheck = deliveryHealthCheck;
            this._logger = logger;
        }

        public bool ProduceEvent(INode node, string methodName)
        {
            if (!this._deliveryHealthCheck.StartupCompleted)
            {
                return false;
            }

            try
            {
                var kafkaObjectCacheEvent = new KafkaChangeCacheEvent() { MethodName = methodName, TypeCacheObject = node.GetType() };
                var message = new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(kafkaObjectCacheEvent) };
                this._kafkaProducer.Produce(this._topic, message);

                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ProduceEventAsync(INode node, string methodName)
        {
            if (!this._deliveryHealthCheck.StartupCompleted)
            {
                return false;
            }
            try
            {
                var kafkaObjectCacheEvent = new KafkaChangeCacheEvent() { MethodName = methodName, TypeCacheObject = node.GetType() };
                var message = new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(kafkaObjectCacheEvent) };
                await this._kafkaProducer.ProduceAsync(this._topic, message);

                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
                return false;
            }
        }
    }
}
