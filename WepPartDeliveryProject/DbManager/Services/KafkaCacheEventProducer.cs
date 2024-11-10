using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Data;
using DbManager.Data.Cache;
using DbManager.Data.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace DbManager.Services
{
    public class KafkaCacheEventProducer
    {
        private readonly KafkaDependentProducer<string, string> _kafkaProducer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly string _topic;
        private readonly DeliveryHealthCheck _deliveryHealthCheck;
        private readonly ILogger<KafkaCacheEventProducer> _logger;
        private readonly Instrumentation _instrumentation;

        public KafkaCacheEventProducer(KafkaDependentProducer<string, string> kafkaProducer, IOptions<KafkaSettings> kafkaOptions, DeliveryHealthCheck deliveryHealthCheck, ILogger<KafkaCacheEventProducer> logger,
            Instrumentation instrumentation)
        {
            this._kafkaSettings = kafkaOptions.Value;
            this._topic = this._kafkaSettings.ContainerEventsTopic ?? "ContainerEvents";
            this._kafkaProducer = kafkaProducer;
            this._deliveryHealthCheck = deliveryHealthCheck;
            this._logger = logger;
            this._instrumentation = instrumentation;
        }

        public bool ProduceEvent(INode node, string methodName)
        {
            if (!this._deliveryHealthCheck.StartupCompleted)
            {
                return false;
            }

            try
            {
                using var activity = this._instrumentation.ActivitySource.StartActivity("ProduceEvent", System.Diagnostics.ActivityKind.Producer);
                activity?.SetTag("node.type", node.GetType().Name);

                var kafkaObjectCacheEvent = new KafkaChangeCacheEvent() { MethodName = methodName, TypeCacheObject = node.GetType() };
                var message = new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(kafkaObjectCacheEvent) };

                if (activity != null)
                    Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), message.Headers ??= new Headers(), (headers, key, value) => headers.Add(key, Encoding.UTF8.GetBytes(value)));
                
                this._kafkaProducer.Produce(this._topic, message, result => this.HandleDeliveryResult(activity, result));

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
                using var activity = this._instrumentation.ActivitySource.StartActivity("ProduceEventAsync", System.Diagnostics.ActivityKind.Producer);
                activity?.SetTag("node.type", node.GetType().Name);

                var kafkaObjectCacheEvent = new KafkaChangeCacheEvent() { MethodName = methodName, TypeCacheObject = node.GetType() };
                var message = new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(kafkaObjectCacheEvent) };

                if (activity != null)
                    Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), message.Headers ??= new Headers(), (headers, key, value) => headers.Add(key, Encoding.UTF8.GetBytes(value)));

                var result = await this._kafkaProducer.ProduceAsync(this._topic, message);
                this.HandleDeliveryResult(activity, result);

                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
                return false;
            }
        }

        private void HandleDeliveryResult(Activity activity, DeliveryResult<string, string> deliveryResult) 
        {
            activity?.SetTag("kafka.topic", deliveryResult.Topic);
            activity?.SetTag("kafka.partition", deliveryResult.Partition.Value);
            activity?.SetTag("kafka.offset", deliveryResult.Offset.Value);
        }
    }
}
