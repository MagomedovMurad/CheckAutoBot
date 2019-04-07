using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class VehicleProperty
    {
        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("pin")]
        public string Pin { get; set; }

        [JsonProperty("chassis")]
        public string Chassis { get; set; }

        [JsonProperty("bodyNum")]
        public string FrameNumber { get; set; }
    }
}
