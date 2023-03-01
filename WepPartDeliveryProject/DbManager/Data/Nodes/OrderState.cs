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

        /// <summary>
        /// Order states loading from DB when app starts
        /// string - NameOfState
        /// OrderState - state from DB
        /// </summary>
        [JsonIgnore]
        public static Dictionary<string, OrderState> OrderStatesFromDb = new Dictionary<string, OrderState>();

        [JsonIgnore]
        public List<Order> Orders { get; set; } 
    }
}
