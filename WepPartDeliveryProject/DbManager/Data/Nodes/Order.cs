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
        public int SumWeight { get; set; }
        public string DeliveryAddress { get; set; }
        public string? ReasonForCancellation { get; set; }

        //Order don't exist
        public DateTime? WasOrdered { get; set; }
        //Order in "kitchen queue" stage (WasOrdered - StartCook)
        public DateTime? StartCook { get; set; }
        //Order in "cooked" stage (StartCook - WasCooked)
        public DateTime? WasCooked { get; set; }
        //Order in "wait delivery man" stage (WasCooked - TakenByDeliveryMan)
        public DateTime? TakenByDeliveryMan { get; set; }
        //Order in "delivered" stage (TakenByDeliveryMan - WasDelivered)
        public DateTime? WasDelivered { get; set; }
        //Order in "closed" stage 

        [JsonIgnore]
        public List<OrderedDish>? OrderedObjects { get; set; }
        [JsonIgnore]
        public DeliveredBy? DeliveredMan { get; set; }
        [JsonIgnore]
        public Ordered Client { get; set; }
        [JsonIgnore]
        public CookedBy? Kitchen { get; set; }

    }
}
