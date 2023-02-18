using DbManager.Data.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class DeliveredBy : Model, IRelation
    {
        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public DeliveryMan DeliveryMan { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => DeliveryMan;
            set => DeliveryMan = (DeliveryMan)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => Order;
            set => Order = (Order)value;
        }
    }
}
