using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class RestrictedAccident
    {
        public string RegionName { get; set; }
        public string RestrictedFoundations { get; set; }
        public string VechicleYear { get; set; }
        public string Vin { get; set; }
        public string RestrictedDate { get; set; }
        public string VechicleModel { get; set; }
        public string FrameNumber { get; set; }
        public string DateAdd { get; set; }
        public string InitiatorPhone { get; set; }
        public string InitiatorType { get; set; }
        public string RestrictedType { get; set; }
    }
}
