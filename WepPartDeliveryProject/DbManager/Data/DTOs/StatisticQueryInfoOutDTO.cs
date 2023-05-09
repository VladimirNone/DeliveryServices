using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class StatisticQueryInfoOutDTO
    {
        public string NameQuery { get;set; }
        public string LinkToQuery { get;set; }
        public string ChartName { get;set; }
        public bool NeedDataRange { get;set; }
        public List<string>? NameDatasets { get; set; }
    }
}
