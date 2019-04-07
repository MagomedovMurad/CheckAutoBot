using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.AvtoYslygaModels
{
    public class DiagnosticCard
    {
        public string BrandAndModel { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Vin { get; set; }

        public string FrameNumber { get; set; }

        public string LicensePlate { get; set; }

        public string EaistoNumber { get; set; }

        public string OperatorName { get; set; }
    }
}
