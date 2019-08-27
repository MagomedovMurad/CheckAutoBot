using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources.Models
{
    public class DataSourceResult
    {
        public object Data { get; set; }

        public IEnumerable<RelatedData> RelatedData { get; set; }
    }
}
