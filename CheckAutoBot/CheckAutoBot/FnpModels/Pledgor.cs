﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class Pledgor
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("privatePerson")]
        public PrivatePerson PrivatePerson { get; set; }

        [JsonProperty("soleProprietorship")]
        public SoleProprietorship SoleProprietorship { get; set; }
    }
}
