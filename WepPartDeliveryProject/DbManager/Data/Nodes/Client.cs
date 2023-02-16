using DbManager.Data.Relations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class Client : User, INode
    {
        public string Name { get; set; }
        public string Address { get; set; }

        [JsonIgnore]
        public List<Ordered>? ClientOrders { get; set; }
    }
}
