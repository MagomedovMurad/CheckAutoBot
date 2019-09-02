using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.EaistoModels
{
    public class DiagnosticCard
    {
        [JsonProperty("mark")]
        public string Brand { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("dkStart")]
        public string DateFrom { get; set; }

        [JsonProperty("dkEnd")]
        public string DateTo { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("bodyNumber")]
        public string FrameNumber { get; set; }

        [JsonProperty("regNumber")]
        public string LicensePlate { get; set; }

        [JsonProperty("dkNumber")]
        public string EaistoNumber { get; set; }

        public string Operator { get; set; }

        public string Expert { get; set; }
    }
}
