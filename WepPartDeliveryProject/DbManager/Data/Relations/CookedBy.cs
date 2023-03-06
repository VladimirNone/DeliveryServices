using DbManager.Data.Nodes;
using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// Kitchen -> Order
    /// </summary>
    public class CookedBy : Relation<Kitchen, Order>
    {
    }
}
