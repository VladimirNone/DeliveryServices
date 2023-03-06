using DbManager.Data.Nodes;
using Neo4jClient;

namespace DbManager.Data.Relations
{
    /// <summary>
    /// Order -> OrderState
    /// </summary>
    public class HasOrderState : Relation<Order, OrderState>
    {
        public DateTime TimeStartState { get; set; }
        public string? Comments { get; set; }

    }
}
