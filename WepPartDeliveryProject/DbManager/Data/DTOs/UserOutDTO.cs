using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class UserOutDTO
    {
        public string Login { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime? Born { get; set; }

        // Client props
        public double? Bonuses { get; set; }

        //KitchenWorker props
        public string? JobTitle { get; set; }

        //Admin props

    }
}
