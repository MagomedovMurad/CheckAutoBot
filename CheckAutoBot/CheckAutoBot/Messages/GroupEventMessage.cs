using CheckAutoBot.Enums;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public JObject JsonObject { get; set; } 

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

    }
}
