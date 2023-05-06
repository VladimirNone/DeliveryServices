using DbManager.Data.Relations;
using Neo4jClient;

namespace DbManager.Data.Nodes
{
    /// <summary>
    /// Продаваемое блюдо (напиток, товар)
    /// </summary>
    public class Dish : Node, INode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Weight { get; set; }
        public bool IsAvailableForUser { get; set; }
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Images of product. First image is main
        /// </summary>
        public List<string> Images { get; set; }

        [Neo4jIgnore]
        public List<OrderedDish>? Orders { get; set; }
    }
}
