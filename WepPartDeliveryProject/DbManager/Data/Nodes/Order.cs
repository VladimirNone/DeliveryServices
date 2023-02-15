using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Relations;

namespace DbManager.Data.Nodes
{
    public class Order : Model, INode
    {
        public double Price { get; set; }

        public List<OrderedProduct> OrderedObjects { get; set; }
    }
}
