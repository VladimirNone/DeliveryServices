using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data
{
    public abstract class Model : IModel
    {
        public Guid Id { get; set; }
    }
}
