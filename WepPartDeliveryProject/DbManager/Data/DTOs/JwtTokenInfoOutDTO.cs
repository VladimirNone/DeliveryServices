using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class JwtTokenInfoOutDTO
    {
        public string JwtToken { get; set; }
        public DateTime ValidTo { get; set; }
        public List<string> RoleNames { get; set; }
    }
}
