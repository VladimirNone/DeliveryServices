using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relationships
{
    public class WorkedIn : IModel
    {
        public Kitchen Kitchen { get; set; }
        public KitchenWorker KitchenWorker { get; set; }
    }
}
