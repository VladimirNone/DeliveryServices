using DbManager.Data.Nodes;
using Neo4jClient;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// KitchenWorker -> Kitchen
    /// </summary>
    public class WorkedIn : Relation<KitchenWorker, Kitchen>
    {
        [Neo4jDateTime]
        public DateTime GotJob { get; set; }
    }
}
