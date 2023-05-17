using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class ManipulateOrderDataInDTO
    {
        public int? NewCount { get; set; }
        public string? OrderId { get; set; }
        public string? DishId { get; set; }
        public string? ReasonOfCancel { get; set; }
    }
}
