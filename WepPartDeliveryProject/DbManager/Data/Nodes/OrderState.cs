using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class OrderState : Model, INode
    {
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }

        [JsonIgnore]
        public List<Order> Orders { get; set; } 
    }
}
