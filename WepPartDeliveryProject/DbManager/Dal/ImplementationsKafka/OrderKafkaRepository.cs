using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Services;

namespace DbManager.Dal.ImplementationsKafka
{
    public class OrderKafkaRepository : OrderRepository
    {
        public OrderKafkaRepository(KafkaEventProducer kafkaProducer, BoltGraphClientFactory boltGraphClientFactory, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._changeCacheEvent += (node, methodName) => kafkaProducer.ProduceOrderAsync((Order)node, methodName);
        }
    }
}
