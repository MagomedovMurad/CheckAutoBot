using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class LinkButton
    {
        /// <summary>
        /// Название кнопки
        /// </summary>
        [JsonProperty("id")]
        public string Title { get; set; }

        /// <summary>
        /// Действие для кнопки
        /// </summary>
        [JsonProperty("action")]
        public LinkButtonAction Action { get; set; }
    }
}
