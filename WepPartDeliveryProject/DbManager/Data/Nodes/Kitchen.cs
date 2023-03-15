using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class Kitchen : Node, INode
    {
        public string Address { get; set; }

        [Neo4jIgnore]
        public List<CookedBy>? PreparedOrders { get; set; }
    }
}
