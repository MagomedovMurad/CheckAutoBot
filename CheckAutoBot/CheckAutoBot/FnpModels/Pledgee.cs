using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class Pledgee
    {
        [JsonProperty("organization")]
        public Organization Organization { get; set; }

        [JsonProperty("privatePerson")]
        public PrivatePerson PrivatePerson { get; set; }
    }
}
