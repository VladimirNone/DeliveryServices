using DbManager.AppSettings;
using DbManager.Data;
using DbManager.Data.Kafka;
using DbManager.Neo4j.Implementations;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace DbManager.Dal.ImplementationsKafka
{
    internal class GeneralKafkaRepository<TNode> : GeneralNeo4jRepository<TNode>
        where TNode : INode
    {
        private readonly KafkaDependentProducer<string, string> _kafkaProducer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly string _topic;
        private int kkk;

        public GeneralKafkaRepository(BoltGraphClientFactory boltGraphClientFactory, KafkaDependentProducer<string, string> kafkaProducer, IOptions<KafkaSettings> kafkaOptions) : base(boltGraphClientFactory)
        {
            this._kafkaSettings = kafkaOptions.Value;
            this._topic = this._kafkaSettings.ContainerEventsTopic ?? "ContainerEvents";
            this._kafkaProducer = kafkaProducer;
        }

        public override async Task AddNodeAsync(INode node)
        {
            await base.AddNodeAsync(node);
            var message = new Confluent.Kafka.Message<string, string>() { Key = node.Id.ToString(), Value = JsonConvert.SerializeObject(node) };
            message.Headers = new Confluent.Kafka.Headers
            {
                { KafkaConsts.OwnerGroupId, Encoding.UTF8.GetBytes(this._kafkaSettings.GroupId) },
                { KafkaConsts.ObjectType, Encoding.UTF8.GetBytes(node.GetType().ToString()) }
            };
            await this._kafkaProducer.ProduceAsync(this._topic, message);
        }

        public override async Task DeleteNodeWithAllRelations(INode node)
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

        public override async Task UpdateNodeAsync(INode node)
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
