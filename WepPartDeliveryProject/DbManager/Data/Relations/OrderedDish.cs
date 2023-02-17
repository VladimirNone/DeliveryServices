using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Nodes;
using Newtonsoft.Json;

namespace DbManager.Data.Relations
{
    public class OrderedDish : Model, IRelation
    {
        public int Count { get; set; }

        [JsonIgnore]
        public Dish OrderedItem { get; set; }
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
            set => OrderedItem = (Dish)value;
        }
    }
}
