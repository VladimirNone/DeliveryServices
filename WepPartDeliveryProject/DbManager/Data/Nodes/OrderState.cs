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
        public int NumberOfStage { get; set; }
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }
        

        [JsonIgnore]
        public static Dictionary<int, OrderState> OrderStatesFromDb = new Dictionary<int, OrderState>();

        [JsonIgnore]
        public List<Order> Orders { get; set; } 
    }
}
