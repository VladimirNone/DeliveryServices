using DbManager.Data.Nodes;
using Newtonsoft.Json;

namespace DbManager.Data.Relations
{
    public class Ordered : Model, IRelation
    {

        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => Client;
            set => Client = (Client)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => Order;
            set => Order = (Order)value;
        }
    }
}
