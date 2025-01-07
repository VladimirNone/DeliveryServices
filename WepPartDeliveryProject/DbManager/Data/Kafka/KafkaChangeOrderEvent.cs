using DbManager.Dal;
using DbManager.Data.Nodes;
using DbManager.Services;
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

        public object TupleMethodParams { get; set; }

        public const string AddMethodName = nameof(IGeneralRepository<Order>.AddNodeAsync);
        public const string UpdateMethodName = nameof(IGeneralRepository<Order>.UpdateNodeAsync);
        public const string TryRemoveMethodName = nameof(IGeneralRepository<Order>.DeleteNodeWithAllRelations);
        public const string RelateNodesMethodName = nameof(IGeneralRepository<Order>.RelateNodesAsync);
        public const string UpdateRelationNodesMethodName = nameof(IGeneralRepository<Order>.UpdateRelationNodesAsync);
        public const string DeleteRelationNodesMethodName = nameof(IGeneralRepository<Order>.DeleteRelationOfNodesAsync);

        public const string CancelOrderMethodName = nameof(IOrderService.CancelOrder);
        public const string CancelOrderedDishMethodName = nameof(IOrderService.CancelOrderedDish);
        public const string ChangeCountOrderedDishMethodName = nameof(IOrderService.ChangeCountOrderedDish);
        public const string MoveOrderToNextStageMethodName = nameof(IOrderService.MoveOrderToNextStage);
        public const string MoveOrderToPreviousStageMethodName = nameof(IOrderService.MoveOrderToPreviousStage);
        public const string PlaceAnOrderMethodName = nameof(IOrderService.PlaceAnOrder);
    }
}
