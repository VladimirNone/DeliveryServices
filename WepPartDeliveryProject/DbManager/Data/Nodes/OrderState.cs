using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class OrderState : Node, INode
    {
        public int NumberOfStage { get; set; }
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }

        [Neo4jIgnore]
        public List<Order> Orders { get; set; } 
    }
}
