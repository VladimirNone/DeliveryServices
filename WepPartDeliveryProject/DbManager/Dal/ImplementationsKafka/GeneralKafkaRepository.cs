using DbManager.Data;
using DbManager.Neo4j.Implementations;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Neo4jClient;

namespace DbManager.Dal.ImplementationsKafka
{
    internal class GeneralKafkaRepository<TNode> : GeneralNeo4jRepository<TNode>
        where TNode : INode
    {
        private readonly KafkaDependentProducer<string, string> _kafkaProducer;
        private readonly string _topic;

        public GeneralKafkaRepository(IGraphClient DbContext, KafkaDependentProducer<string, string> kafkaProducer, IConfiguration configuration) : base(DbContext)
        {
            this._topic = configuration["ContainerEventsTopic"] ?? "ContainerEvents";
            this._kafkaProducer = kafkaProducer;
        }

        public override async Task AddNodeAsync(TNode node)
        {
            await base.AddNodeAsync(node);
            await this._kafkaProducer.ProduceAsync(this._topic, new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(),  Value = node?.GetType()?.Name ?? "node is  null" });
        }

        public override async Task AddNodesAsync(List<TNode> newNodes)
        {
            await base.AddNodesAsync(newNodes);
        }

        public override async Task DeleteNodeWithAllRelations(TNode node)
        {
            await base.DeleteNodeWithAllRelations(node);
        }

        public override async Task DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(TNode node, TRelatedNode relatedNode)
        {
            await base.DeleteRelationOfNodesAsync<TRelation, TRelatedNode>(node, relatedNode);
        }

        public override async Task RelateNodesAsync<TRelation>(TRelation relation)
        {
            await base.RelateNodesAsync(relation);
        }

        public override async Task RemoveNodeType<TNodeType>(string nodeId)
        {
            await base.RemoveNodeType<TNodeType>(nodeId);
        }

        public override async Task RemoveNodeType(string nodeId, string nodeTypeName)
        {
            await base.RemoveNodeType(nodeId, nodeTypeName);
        }

        public override async Task SetNewNodeType<TNewNodeType>(string nodeId)
        {
            await base.SetNewNodeType<TNewNodeType>(nodeId);
        }

        public override async Task SetNewNodeType(string nodeId, string nodeTypeName)
        {
            await base.SetNewNodeType(nodeId, nodeTypeName);
        }

        public override async Task UpdateNodeAsync(TNode node)
        {
            await base.UpdateNodeAsync(node);
        }

        public override async Task UpdateNodesPropertiesAsync(TNode node)
        {
            await base.UpdateNodesPropertiesAsync(node);
        }

        public override async Task UpdateRelationNodesAsync<TRelation>(TRelation updatedRelation)
        {
            await base.UpdateRelationNodesAsync(updatedRelation);
        }
    }
}
