
namespace DbManager.Data.Nodes
{
    public class Category : Model, INode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryNumber { get; set; }
    }
}
