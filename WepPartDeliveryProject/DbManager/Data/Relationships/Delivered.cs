using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relationships
{
    public class Delivered : IModel
    {
        public Order Order { get; set; }
        public DeliveryMan DeliveryMan { get; set; }

    }
}
