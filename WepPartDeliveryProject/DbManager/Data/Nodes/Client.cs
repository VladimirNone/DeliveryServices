using DbManager.Data.Relations;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class Client : User, INode
    {
        public float Bonuses { get; set; }

        [JsonIgnore]
        public List<Ordered>? ClientOrders { get; set; }
    }
}
