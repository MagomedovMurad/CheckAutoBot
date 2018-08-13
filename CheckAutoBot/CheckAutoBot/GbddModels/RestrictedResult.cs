using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class RestrictedResult
    {
        [JsonProperty("records")]
        public List<Restricted> Records { get; set; }

        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
