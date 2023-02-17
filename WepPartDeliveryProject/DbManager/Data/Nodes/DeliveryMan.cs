using DbManager.Data.Relations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class DeliveryMan : User, INode
    {
        public int MaxWeight { get; set; }

        [JsonIgnore]
        public List<DeliveredBy>? DeliveredOrders { get; set; }
    }
}
