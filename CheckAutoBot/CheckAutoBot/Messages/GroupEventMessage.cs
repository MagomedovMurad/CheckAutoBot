using CheckAutoBot.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CheckAutoBot.Messages
{
    [Obsolete("Заменен на GroupEventEnvelop")]
    public class GroupEventMessage
    {
        [JsonProperty("type")]
        public VkEventType EventType { get; set; }

        [JsonProperty("object")]
        public JObject JsonObject { get; set; } 

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

    }
}
