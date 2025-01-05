using DbManager.Neo4j.Implementations;
using DbManager.Services;

namespace DbManager.Dal.ImplementationsKafka
{
    public class DishKafkaRepository : DishRepository
    {
        public DishKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            //в теории не потеряем (я про gc) kafkaProducer, т.к. он синглтон
            this._changeCacheEvent += kafkaProducer.ProduceEventAsync;
        }
    }
}
