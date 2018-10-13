using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class WantedResult
    {
        [JsonProperty("records")]
        public List<Wanted> Wanteds { get; set; }

        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
