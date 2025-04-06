using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class OrderState : Node, INode
    {
        public static List<OrderState> OrderStatesFromDb { get; set; }
        public int NumberOfStage { get; set; }
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<Order> Orders { get; set; } 
    }
}
