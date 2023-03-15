using Neo4jClient;
using Newtonsoft.Json;

namespace DbManager.Data
{
    public class Relation<TFrom, TTo> : IRelation where TFrom : class, INode where TTo : class, INode
    {
        public Guid Id { get; set; }

        private TFrom? _nodeFrom;
        private TTo? _nodeTo;

        [Neo4jIgnore]
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
