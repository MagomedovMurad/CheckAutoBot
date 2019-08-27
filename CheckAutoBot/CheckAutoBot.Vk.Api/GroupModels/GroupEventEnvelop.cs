using CheckAutoBot.Vk.Api.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.GroupModels
{
    public class GroupEventEnvelop
    {
        [JsonProperty("type")]
        public EventType EventType { get; set; }

        [JsonProperty("object")]
        public JObject JsonObject { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }
    }
}
