using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class OrderedBy : Model, IRelation
    {
        public Order Order { get; set; }
        public Client Client { get; set; }

        public string SomeText { get; set; }

        public INode NodeFrom => Client;

        public INode NodeTo => Order;
    }
}
