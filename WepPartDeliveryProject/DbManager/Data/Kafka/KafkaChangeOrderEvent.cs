using DbManager.Dal;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using Newtonsoft.Json;

namespace DbManager.Data.Kafka
{
    public class KafkaChangeOrderEvent
    {
        public Order Order { get; set; }
        public string MethodName { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public IRelation Relation { get; set; }
        public Type RelationType { get; set; }

        public string RelationJson
        {
            get => JsonConvert.SerializeObject(this.Relation, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            set => Relation = (IRelation)JsonConvert.DeserializeObject(value, this.RelationType, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

        public const string AddMethodName = nameof(IGeneralRepository<Order>.AddNodeAsync);
        public const string UpdateMethodName = nameof(IGeneralRepository<Order>.UpdateNodeAsync);
        public const string TryRemoveMethodName = nameof(IGeneralRepository<Order>.DeleteNodeWithAllRelations);
        public const string RelateNodesMethodName = nameof(IGeneralRepository<Order>.RelateNodesAsync);
    }
}
