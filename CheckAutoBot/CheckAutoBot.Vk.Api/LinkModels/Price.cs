using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class Price
    {
        /// <summary>
        /// Целочисленное значение цены, умноженное на 100
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        [JsonProperty("currency")]
        public Currency Currency { get; set; }

        /// <summary>
        /// Строка с локализованной ценой и валютой
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
