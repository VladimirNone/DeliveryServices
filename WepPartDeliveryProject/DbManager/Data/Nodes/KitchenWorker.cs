using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    public class KitchenWorker : User, INode
    {
        public string JobTitle { get; set; }

        [Neo4jIgnore]
        public WorkedIn? Kitchen { get; set; }
    }
}
