using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class Pledge
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("regDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonProperty("properties")]
        public List<VechicleProperty> Properties { get; set; }

        [JsonProperty("pledgors")]
        public List<Pledgor> Pledgors { get; set; }

        [JsonProperty("pledgees")]
        public List<Pledgee> Pledgees { get; set; }

        [JsonProperty("history")]
        public List<HistoryItem> History { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
