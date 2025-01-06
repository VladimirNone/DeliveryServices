using DbManager.Data.Cache;
using DbManager.Data.Nodes;

namespace DbManager.Data.Kafka
{
    public class KafkaChangeCacheEvent
    {
        public Type TypeObject { get; set; }
        public string MethodName { get; set; }

        public const string AddMethodName = nameof(ObjectCache<INode>.Add);
        public const string UpdateMethodName = nameof(ObjectCache<INode>.Update);
        public const string TryRemoveMethodName = nameof(ObjectCache<INode>.TryRemove);
    }
}
