using DbManager.Data;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Services;

namespace DbManager.Dal.ImplementationsKafka
{
    public class OrderKafkaRepository : OrderRepository
    {
        private readonly KafkaEventProducer _kafkaProducer;

        public OrderKafkaRepository(KafkaEventProducer kafkaProducer, BoltGraphClientFactory boltGraphClientFactory, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._kafkaProducer = kafkaProducer;
        }

        public override async Task AddNodeAsync(INode node)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = (Order)node, MethodName = KafkaChangeOrderEvent.AddMethodName });
        }

        public override async Task DeleteNodeWithAllRelations(INode node)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = (Order)node, MethodName = KafkaChangeOrderEvent.TryRemoveMethodName });
        }

        public override async Task UpdateNodeAsync(INode node)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = (Order)node, MethodName = KafkaChangeOrderEvent.UpdateMethodName });
        }

        public override async Task RelateNodesAsync<TRelation>(TRelation relation)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { MethodName = KafkaChangeOrderEvent.RelateNodesMethodName, Relation = relation, RelationType = relation.GetType() });
        }
    }
}
