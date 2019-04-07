using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeListItem1
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
        public List<PledgeProperty1> Properties { get; set; }

        /// <summary>
        /// Зологодатели
        /// </summary>
        [JsonProperty("pledgors")]
        public List<Pledgor1> Pledgors { get; set; }

        /// <summary>
        /// Залогодержатели
        /// </summary>
        [JsonProperty("pledgees")]
        public List<Pledgee1> Pledgees { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("notificationDataUrl")]
        public string NotificationDataUrl { get; set; }
    }
}
