using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class PlaceAnOrderInDTO
    {
        public string DeliveryAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string? Comment { get; set; }  
    }
}
