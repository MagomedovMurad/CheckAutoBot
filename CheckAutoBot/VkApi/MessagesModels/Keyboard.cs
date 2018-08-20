using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VkApi.MessagesModels
{
    public class Keyboard
    {
        [JsonProperty("one_time")]
        public bool OneTime { get; set; }

        [JsonProperty("buttons")]
        public Button[][] Buttons { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
