using CheckAutoBot.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class GroupEventMessage
    {
        [JsonProperty("type")]
        public VkEventType EventType { get; set; }

        [JsonProperty("object")]
        public string JsonObject { get; set; } 

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

    }
}
