using DbManager.Data;
using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Implementations;
using DbManager.Services.Kafka;

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
            if (relation != null)
                throw new ArgumentNullException(nameof(relation));

            var typeOfFirstGenericType = relation?.GetType()?.BaseType?.GenericTypeArguments[0];
            if(typeOfFirstGenericType != null) 
                throw new ArgumentNullException(nameof(typeOfFirstGenericType));

            var orderId = typeOfFirstGenericType == typeof(Order) ? relation.NodeFromId : relation.NodeToId;


            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = (Guid)orderId }, MethodName = KafkaChangeOrderEvent.RelateNodesMethodName, Relation = relation, RelationType = relation.GetType() });
        }

        //Фактически используется только в бэке в воркере, там юзается neo4j и этот метод не вызывается
        public override async Task UpdateRelationNodesAsync<TRelation>(TRelation relation)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { MethodName = KafkaChangeOrderEvent.UpdateRelationNodesMethodName, Relation = relation, RelationType = relation.GetType() });
        }

        public override async Task DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(Order node, TRelatedNode relatedNode)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = node.Id }, MethodName = KafkaChangeOrderEvent.DeleteRelationNodesMethodName, Relation = new Relation<Node, Node>() { NodeFrom = node, NodeTo = relatedNode}, RelationType = typeof(TRelation) });
        }

        //Фактически используется только в бэке в воркере, там юзается neo4j и этот метод не вызывается
        public override async Task DeleteRelationOfNodesAsync<TRelation>(TRelation relation)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { MethodName = KafkaChangeOrderEvent.DeleteRelationNodesMethodName, Relation = relation, RelationType = relation.GetType() });
        }
    }
}
