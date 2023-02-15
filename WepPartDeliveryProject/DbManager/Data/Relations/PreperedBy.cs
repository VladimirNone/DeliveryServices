using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class PreperedBy : Model, IRelation
    {
        public Order Order { get; set; }
        public Kitchen Kitchen { get; set; }

        public INode NodeFrom => Kitchen;

        public INode NodeTo => Order;
    }
}
