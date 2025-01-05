using DbManager.Neo4j.Implementations;
using DbManager.Services;

namespace DbManager.Dal.ImplementationsKafka
{
    public class DeliveryManKafkaRepository : DeliveryManRepository
    {
        public DeliveryManKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._changeCacheEvent += kafkaProducer.ProduceEventAsync;
        }
    }
}
