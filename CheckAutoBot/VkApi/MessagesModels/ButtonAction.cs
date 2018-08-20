using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace VkApi.MessagesModels
{
    public class ButtonAction
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ButtonActionType Type { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("label")]
        public string Lable { get; set; }
    }
}
