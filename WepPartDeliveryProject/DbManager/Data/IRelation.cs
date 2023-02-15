using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data
{
    public interface IRelation
    {
        INode NodeFrom { get; }
        INode NodeTo { get; }
    }
}
