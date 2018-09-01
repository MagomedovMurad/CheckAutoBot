using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class Currency
    {
        /// <summary>
        /// Идентификатор валюты
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Буквенное обозначение валюты
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
