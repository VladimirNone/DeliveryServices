using DbManager.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    public class WorkedIn : Model, IRelation
    {
        public Kitchen Kitchen { get; set; }
        public KitchenWorker KitchenWorker { get; set; }

        public INode NodeFrom => KitchenWorker;

        public INode NodeTo => Kitchen;
    }
}
