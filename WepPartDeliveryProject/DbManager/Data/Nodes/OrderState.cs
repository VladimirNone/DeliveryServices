using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class OrderState : Model, INode
    {
        public int NumberOfStage { get; set; }
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }

        /// <summary>
        /// Order states loading from DB when app starts
        /// string - NameOfState
        /// OrderState - state from DB
        /// </summary>
        [JsonIgnore]
        [Neo4jIgnore]
        public static List<OrderState> OrderStatesFromDb = new List<OrderState>();

        [Neo4jIgnore]
        public List<Order> Orders { get; set; } 
    }
}
