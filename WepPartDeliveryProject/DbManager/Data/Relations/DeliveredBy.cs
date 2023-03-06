using DbManager.Data.Nodes;
using Neo4jClient;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// DeliveryMan -> Order
    /// </summary>
    public class DeliveredBy : Relation<DeliveryMan, Order>
    {

    }
}
