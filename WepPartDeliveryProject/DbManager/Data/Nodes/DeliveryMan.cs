using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class DeliveryMan : User, INode
    {
        public int MaxWeight { get; set; }

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<DeliveredBy>? DeliveredOrders { get; set; }
    }
}
