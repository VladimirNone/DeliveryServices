using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class ManipulateDishDataInDTO
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public int? Weight { get; set; }
        public bool? IsAvailable { get; set; }

        public List<string>? Images { get; set; }
    }
}
