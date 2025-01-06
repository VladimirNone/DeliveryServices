using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class Order : Node, INode
    {
        public double Price { get; set; }
        public int SumWeight { get; set; }
        public string DeliveryAddress { get; set; }
        public string PhoneNumber { get; set; }

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<HasOrderState> Story { get; set; } = new List<HasOrderState>();

        public string StoryJson
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(Story, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
            }
            set
            {
                Story = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HasOrderState>>(value, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            }
        }

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<OrderedDish>? OrderedObjects { get; set; }
        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DeliveredBy? DeliveredMan { get; set; }
        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Ordered Client { get; set; }
        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public CookedBy? Kitchen { get; set; }

    }
}
