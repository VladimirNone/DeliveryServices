using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Nodes;

namespace DbManager.Data.Relations
{
    public class OrderedProduct : Model, IRelation
    {
        public Product OrderedItem { get; set; }
        public Order Order { get; set; }

        public INode NodeFrom => OrderedItem;

        public INode NodeTo => Order;

        public int Count { get; set; }
    }
}
