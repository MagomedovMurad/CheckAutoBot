﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class Pledge
    {
        [JsonProperty("type")]
        public SubjectType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
