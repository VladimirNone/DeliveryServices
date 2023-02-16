using DbManager.Data.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class KitchenWorker : User, INode
    {

        public WorkedIn? Kitchen { get; set; }
    }
}
