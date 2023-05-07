using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class ManipulateDishDataInDTO
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public int? Weight { get; set; }
        public bool? IsAvailableForUser { get; set; }
        public bool? IsDeleted { get; set; }

        public string? CategoryId { get; set; }
        public IFormFileCollection? ImagesFiles { get; set; }
    }
}
