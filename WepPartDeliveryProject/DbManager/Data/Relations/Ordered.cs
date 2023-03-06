using DbManager.Data.Nodes;
using Neo4jClient;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// Client -> Order
    /// </summary>
    public class Ordered : Relation<Client, Order>
    {

    }
}
