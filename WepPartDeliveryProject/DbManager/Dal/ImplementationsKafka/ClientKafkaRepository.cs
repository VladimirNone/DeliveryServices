using DbManager.Neo4j.Implementations;
using DbManager.Services.Kafka;

namespace DbManager.Dal.ImplementationsKafka
{
    public class ClientKafkaRepository : ClientRepository
    {
        public ClientKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._changeCacheEvent += kafkaProducer.ProduceEventAsync;
        }
    }
}
