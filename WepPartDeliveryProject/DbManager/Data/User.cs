using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data
{
    public abstract class User : Model
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
    }
}
