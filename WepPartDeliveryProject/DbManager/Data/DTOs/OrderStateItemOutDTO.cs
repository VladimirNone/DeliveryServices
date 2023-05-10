using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class OrderStateItemOutDTO
    {
        public DateTime? TimeStartState { get; set; }
        public string? Comment { get; set; }
        public Guid OrderStateId { get; set; }
        public int NumberOfStage { get; set; }
        public string NameOfState { get; set; }
        public string DescriptionForClient { get; set; }
    }
}
