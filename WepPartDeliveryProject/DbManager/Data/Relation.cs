using Neo4jClient;

namespace DbManager.Data
{
    public class Relation<TFrom, TTo> : IRelation where TFrom : class, INode where TTo : class, INode
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private TFrom? _nodeFrom;
        private TTo? _nodeTo;

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public INode NodeFrom
        {
            get => _nodeFrom;
            set
            {
                NodeFromId = value.Id;
                _nodeFrom = (TFrom)value;
            }
        }

        [Neo4jIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public INode NodeTo
        {
            get => _nodeTo;
            set
            {
                NodeToId = value.Id;
                _nodeTo = (TTo)value;
            }
        }

        public Guid? NodeFromId { get; set; }
        public Guid? NodeToId { get; set; }

    }
}
