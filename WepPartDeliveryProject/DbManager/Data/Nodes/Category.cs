
using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class Category : Node, INode
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string LinkName { get; set; }
        public int CategoryNumber { get; set; }
    }
}
