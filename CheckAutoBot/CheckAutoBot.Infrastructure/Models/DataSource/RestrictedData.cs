using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class RestrictedData
    {
        public IEnumerable<RestrictedAccident> Accidents { get; set; }
    }
}
