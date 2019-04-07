using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class Filter
    {
        [JsonProperty("property")]
        public FilterProperty Property { get; set; }
    }
}
