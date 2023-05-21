using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class ReviewOrderInDTO
    {
        public string? OrderId { get; set; }
        public int? ClientRating { get; set; }
        public string? Review { get; set; }
    }
}
