using DbManager.Data.Cache;

namespace DbManager.Data.Kafka
{
    public class KafkaChangeCacheEvent
    {
        public Type TypeCacheObject { get; set; }
        public string MethodName { get; set; }

        public const string AddMethodName = nameof(ObjectCache<INode>.Add);
        public const string UpdateMethodName = nameof(ObjectCache<INode>.Update);
        public const string TryRemoveMethodName = nameof(ObjectCache<INode>.TryRemove);
    }


}
