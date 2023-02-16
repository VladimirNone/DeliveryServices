using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Relations;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class Order : Model, INode
    {
        public double Price { get; set; }

        [JsonIgnore]
        public List<OrderedProduct>? OrderedObjects { get; set; }
        [JsonIgnore]
        public DeliveredBy? DeliveredMan { get; set; }
        [JsonIgnore]
        public Ordered Client { get; set; }
        [JsonIgnore]
        public PreparedBy? Kitchen { get; set; }
    }
}
