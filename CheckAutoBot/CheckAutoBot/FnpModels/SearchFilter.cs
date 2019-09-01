﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class SearchFilter
    {
        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SearchMode Mode { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }
}