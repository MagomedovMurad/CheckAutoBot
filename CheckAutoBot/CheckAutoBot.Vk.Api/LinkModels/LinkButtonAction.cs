using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class LinkButtonAction
    {
        /// <summary>
        /// Tип действия
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// URL для перехода
        /// </summary>
        [JsonProperty("url")]
        public string Url
        {
            get
            {
                return "open_url";
            }
        }
    }
}
