using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class WantedData
    {
        public IEnumerable<WantedAccident> Accidents { get; set; }
    }
}
