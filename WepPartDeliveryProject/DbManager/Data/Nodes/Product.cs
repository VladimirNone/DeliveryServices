using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.Nodes
{
    /// <summary>
    /// Продаваемое блюдо (напиток, товар)
    /// </summary>
    public class Product : Model, INode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string PathToMainImage { get; set; }
    }
}
