using DbManager.Data.Nodes;
using Neo4jClient;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// Client -> Order
    /// </summary>
    public class ReviewedBy : Relation<Client, Order>
    {
        public int ClientRating { get; set; }
        public DateTime TimeCreated { get; set; }
        public string? Review { get; set; }
    }
}
