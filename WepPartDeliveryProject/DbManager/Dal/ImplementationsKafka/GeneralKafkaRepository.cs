using DbManager.AppSettings;
using DbManager.Data;
using DbManager.Data.Cache;
using DbManager.Data.Kafka;
using DbManager.Neo4j.Implementations;
using DbManager.Services.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace DbManager.Dal.ImplementationsKafka
{
    public class GeneralKafkaRepository<TNode> : GeneralNeo4jRepository<TNode>
        where TNode : INode
    {
        private readonly KafkaEventProducer _kafkaProducer;

        public GeneralKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, Instrumentation instrumentation) : base(boltGraphClientFactory, instrumentation)
        {
            this._kafkaProducer = kafkaProducer;
        }

        public override async Task AddNodeAsync(INode node)
        {
            await base.AddNodeAsync(node);

            await this._kafkaProducer.ProduceEventAsync(node, KafkaChangeCacheEvent.AddMethodName);
        }

        public override async Task DeleteNodeWithAllRelations(INode node)
        {
            await base.DeleteNodeWithAllRelations(node);

            await this._kafkaProducer.ProduceEventAsync(node, KafkaChangeCacheEvent.TryRemoveMethodName);
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

        public override async Task UpdateNodeAsync(INode node)
        {
            await base.UpdateNodeAsync(node);

            await this._kafkaProducer.ProduceEventAsync(node, KafkaChangeCacheEvent.UpdateMethodName);
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
