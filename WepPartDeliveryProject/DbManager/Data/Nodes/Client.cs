using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class Client : User, INode
    {
        public double Bonuses { get; set; }

        [Neo4jIgnore]
        public List<Ordered>? ClientOrders { get; set; }
    }
}
