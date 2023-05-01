using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class JwtTokenInfoDTO
    {
        public string JwtToken { get; set; }
        public DateTime ValidTo { get; set; }
        public string RoleName { get; set; }
    }
}
