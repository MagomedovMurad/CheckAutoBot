using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class DiagnosticCard
    {
        public string Brand { get; set; }

        public string Model { get; set; }

        public string Vin { get; set; }

        public string FrameNumber { get; set; } 

        public string LicensePlate { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string EaistoNumber { get; set; }

        public string Operator { get; set; }
    }
}
