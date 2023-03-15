
using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data.Nodes
{
    public class Category : Node, INode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryNumber { get; set; }

        /// <summary>
        /// Categories loading from DB when app starts
        /// </summary>
        [JsonIgnore]
        [Neo4jIgnore]
        public static List<Category> CategoriesFromDb = new List<Category>();
    }
}
