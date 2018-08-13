using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class VehiclePassport
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("issue")]
        public string CompanyName { get; set; }
    }
}
