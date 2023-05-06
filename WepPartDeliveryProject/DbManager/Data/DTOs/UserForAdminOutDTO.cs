using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class UserForAdminOutDTO
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public DateTime? Born { get; set; }
        public bool IsBlocked { get; set; }

        public string Roles { get; set; }
    }
}
