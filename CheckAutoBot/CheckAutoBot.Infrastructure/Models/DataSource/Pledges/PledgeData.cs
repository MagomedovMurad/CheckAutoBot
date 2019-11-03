using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class PledgeData
    {
        public PledgeData()
        {
            Accidents = new List<PledgeAccident>();
        }
        public IEnumerable<PledgeAccident> Accidents { get; set; }
    }
}
