using Neo4jClient;
using DbManager.Data.Nodes;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// Order -> Dish
    /// </summary>
    public class OrderedDish : Relation<Order, Dish>
    {
        public int Count { get; set; }
    }
}
