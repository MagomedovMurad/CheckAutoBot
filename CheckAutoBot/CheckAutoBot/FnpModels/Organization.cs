using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class Organization
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("publicationDisclaimer")]
        public bool PublicationDisclaimer { get; set; }
    }
}
