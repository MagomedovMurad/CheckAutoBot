using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.MessagesModels
{
    public class Button
    {
        [JsonProperty("action")]
        public ButtonAction Action { get; set; }

        [JsonProperty("color")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ButtonColor Color { get; set; }

        /// <summary>
        /// Возвращает массив с эти элементом
        /// </summary>
        public Button[] ToArray()
        {
            return new[] { this };
        }
    }
}
