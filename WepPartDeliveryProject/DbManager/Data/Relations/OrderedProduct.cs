using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Nodes;
using Newtonsoft.Json;

namespace DbManager.Data.Relations
{
    public class OrderedProduct : Model, IRelation
    {
        [JsonIgnore]
        public Product OrderedItem { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => Order;
            set => Order = (Order)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => OrderedItem;
            set => OrderedItem = (Product)value;
        }

        public int Count { get; set; }
    }
}
