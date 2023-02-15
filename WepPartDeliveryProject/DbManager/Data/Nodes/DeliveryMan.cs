using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class DeliveryMan : User, INode
    {
        public string Name { get; set; }
        public DateTime Born { get; set; }
    }
}
