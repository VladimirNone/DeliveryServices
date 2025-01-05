using DbManager.Neo4j.Implementations;
using DbManager.Services;

namespace DbManager.Dal.ImplementationsKafka
{
    public class UserKafkaRepository : UserRepository
    {
        public UserKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._changeCacheEvent += kafkaProducer.ProduceEventAsync;
        }
    }
}
