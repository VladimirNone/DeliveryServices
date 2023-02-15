using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    public class Client : IUser
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
