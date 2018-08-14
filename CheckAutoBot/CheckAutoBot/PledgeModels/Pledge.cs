using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class Pledge
    {
        [JsonProperty("index")]
        public string index { get; set; }

        [JsonProperty("registerDate")]
        public string RegisterDate { get; set; }

        [JsonProperty("referenceNumber")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("propertiesRemained")]
        public string PropertiesRemained { get; set; }

        [JsonProperty("properties")]
        public string Properties { get; set; }

        [JsonProperty("pledgors")]
        public string Pledgors { get; set; }

        [JsonProperty("pledgees")]
        public string Pledgees { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("notificationDataUrl")]
        public string NotificationDataUrl { get; set; }
    }
}
