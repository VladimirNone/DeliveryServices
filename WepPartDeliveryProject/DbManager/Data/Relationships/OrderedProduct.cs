using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbManager.Data.Nodes;

namespace DbManager.Data.Relationships
{
    public class OrderedProduct : IModel
    {
        public Product OrderedItem { get; set; }
        public Order Order { get; set; }

        public int Count { get; set; }
    }
}
