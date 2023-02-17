using DbManager.Data.Nodes;
using Newtonsoft.Json;

namespace DbManager.Data.Relations
{
    public class ReviewedBy : Model, IRelation
    {
        public int ClientRating { get; set; }
        public DateTime TimeCreated { get; set; }
        public string? Review { get; set; }

        [JsonIgnore]
        public Client Reviewer { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public INode NodeFrom 
        { 
            get => Reviewer; 
            set => Reviewer = (Client)value; 
        }
        [JsonIgnore]
        public INode NodeTo 
        { 
            get => Order; 
            set => Order = (Order)value; 
        }
    }
}
