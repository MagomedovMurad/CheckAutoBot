using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeProperty1
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
