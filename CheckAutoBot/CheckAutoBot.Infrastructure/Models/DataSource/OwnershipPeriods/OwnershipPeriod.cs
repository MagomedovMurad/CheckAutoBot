using CheckAutoBot.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class OwnershipPeriod
    {
        public OwnerType OwnerType { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string LastOperation { get; set; }

        public int LastOperationCode { get; set; }
    }
}
