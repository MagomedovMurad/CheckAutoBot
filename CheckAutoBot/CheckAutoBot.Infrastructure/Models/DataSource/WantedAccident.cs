using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class WantedAccident
    {
        public int Order { get; set; }
        public string RegionIniciator { get; set; }
        public string VechicleModel { get; set; }
        public string Date { get; set; }
        public string VechicleYear { get; set; }
        public string RecordType { get; set; }
    }
}
