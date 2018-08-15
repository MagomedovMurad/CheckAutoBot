﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeListItem
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
        public List<PledgeProperty> Properties { get; set; }

        [JsonProperty("pledgors")]
        public List<Pledgor> Pledgors { get; set; }

        [JsonProperty("pledgees")]
        public List<Pledge> Pledgees { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("notificationDataUrl")]
        public string NotificationDataUrl { get; set; }
    }
}
