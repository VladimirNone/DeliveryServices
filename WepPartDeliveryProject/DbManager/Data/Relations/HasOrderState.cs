using DbManager.Data.Nodes;
using Newtonsoft.Json;

namespace DbManager.Data.Relations
{
    public class HasOrderState : Model, IRelation
    {
        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public OrderState State { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => Order;
            set => Order = (Order)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => State;
            set => State = (OrderState)value;
        }
    }
}
