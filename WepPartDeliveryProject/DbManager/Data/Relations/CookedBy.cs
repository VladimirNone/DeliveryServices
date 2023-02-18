using DbManager.Data.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class CookedBy : Model, IRelation
    {

        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public Kitchen Kitchen { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => Kitchen;
            set => Kitchen = (Kitchen)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => Order;
            set => Order = (Order)value;
        }
    }
}
