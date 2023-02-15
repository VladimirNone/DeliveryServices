using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class Delivered : Model, IRelation
    {
        public Order Order { get; set; }
        public DeliveryMan DeliveryMan { get; set; }

        public INode NodeFrom => DeliveryMan;

        public INode NodeTo => Order;
    }
}
