using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class Product
    {
        /// <summary>
        /// Цена
        /// </summary>
        [JsonProperty("price")]
        public Price Price { get; set; }
    }
}
