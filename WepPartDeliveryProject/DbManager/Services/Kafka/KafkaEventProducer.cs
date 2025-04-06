using Confluent.Kafka;
using DbManager.AppSettings;
using DbManager.Data;

using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace DbManager.Services.Kafka
{
    public class KafkaEventProducer
    {
        private readonly KafkaDependentProducer<string, string> _kafkaProducer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly string _eventTopic;
        private readonly string _orderTopic;
        private readonly DeliveryHealthCheck _deliveryHealthCheck;
        private readonly ILogger<KafkaEventProducer> _logger;
        private readonly Instrumentation _instrumentation;

        public KafkaEventProducer(KafkaDependentProducer<string, string> kafkaProducer, IOptions<KafkaSettings> kafkaOptions, DeliveryHealthCheck deliveryHealthCheck, ILogger<KafkaEventProducer> logger,
            Instrumentation instrumentation)
        {
            _kafkaSettings = kafkaOptions.Value;
            _eventTopic = _kafkaSettings.ContainerEventsTopic ?? "ContainerEvents";
            _orderTopic = _kafkaSettings.ContainerOrdersTopic ?? "ContainerOrders";
            _kafkaProducer = kafkaProducer;
            _deliveryHealthCheck = deliveryHealthCheck;
            _logger = logger;
            _instrumentation = instrumentation;
        }

        public async Task<bool> ProduceEventAsync(INode node, string methodName)
        {
            if (!_deliveryHealthCheck.StartupCompleted)
            {
                return false;
            }
            try
            {
                using var activity = _instrumentation.ActivitySource.StartActivity(nameof(ProduceEventAsync), ActivityKind.Producer);
                activity?.SetTag("node.type", node.GetType().Name);

                var kafkaObjectCacheEvent = new KafkaChangeCacheEvent() { MethodName = methodName, TypeObject = node.GetType() };
                var message = new Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(kafkaObjectCacheEvent) };

                if (activity != null)
                    Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), message.Headers ??= new Headers(), (headers, key, value) => headers.Add(key, Encoding.UTF8.GetBytes(value)));

                var result = await _kafkaProducer.ProduceAsync(_eventTopic, message);
                HandleDeliveryResult(activity, result);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ProduceOrderAsync(KafkaChangeOrderEvent kafkaChangeOrderEvent)
        {
            if (!_deliveryHealthCheck.StartupCompleted)
            {
                return false;
            }
            try
            {
                using var activity = _instrumentation.ActivitySource.StartActivity(nameof(ProduceOrderAsync), ActivityKind.Producer);
                activity?.SetTag("node.type", kafkaChangeOrderEvent.Order?.GetType().Name ?? kafkaChangeOrderEvent.RelationType?.Name);

                var message = new Message<string, string>() { 
                    Key = kafkaChangeOrderEvent.Order?.Id.ToString() ?? kafkaChangeOrderEvent.Relation?.Id.ToString() ?? Guid.Empty.ToString(), 
                    Value = JsonConvert.SerializeObject(kafkaChangeOrderEvent) 
                };

                if (activity != null)
                    Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), message.Headers ??= new Headers(), (headers, key, value) => headers.Add(key, Encoding.UTF8.GetBytes(value)));

                var result = await _kafkaProducer.ProduceAsync(_orderTopic, message);
                HandleDeliveryResult(activity, result);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
