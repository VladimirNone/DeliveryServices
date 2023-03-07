using DbManager.Data.Relations;
using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class Order : Model, INode
    {
        public double Price { get; set; }
        public int SumWeight { get; set; }
        public string DeliveryAddress { get; set; }

        [Neo4jIgnore]
        public List<HasOrderState> Story { get; set; } = new List<HasOrderState>();

        public string StoryJson
        {
            get
            {
                return JsonConvert.SerializeObject(Story, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore});
            }
            set
            {
                Story = JsonConvert.DeserializeObject<List<HasOrderState>>(value, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }
        }

        [Neo4jIgnore]
        public List<OrderedDish>? OrderedObjects { get; set; }
        [Neo4jIgnore]
        public DeliveredBy? DeliveredMan { get; set; }
        [Neo4jIgnore]
        public Ordered Client { get; set; }
        [Neo4jIgnore]
        public CookedBy? Kitchen { get; set; }

    }
}
