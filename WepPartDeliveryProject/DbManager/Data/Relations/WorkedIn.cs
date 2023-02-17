using DbManager.Data.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class WorkedIn : Model, IRelation
    {
        public DateTime? GotJob { get; set; }

        [JsonIgnore]
        public Kitchen Kitchen { get; set; }
        [JsonIgnore]
        public KitchenWorker KitchenWorker { get; set; }
        [JsonIgnore]
        public INode NodeFrom
        {
            get => KitchenWorker;
            set => KitchenWorker = (KitchenWorker)value;
        }
        [JsonIgnore]
        public INode NodeTo
        {
            get => Kitchen;
            set => Kitchen = (Kitchen)value;
        }
    }
}
