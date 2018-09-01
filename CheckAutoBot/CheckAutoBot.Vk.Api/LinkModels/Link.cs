using CheckAutoBot.Vk.Api.PhotosModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.LinkModels
{
    public class Link
    {
        /// <summary>
        /// URL ссылки
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Заголовок ссылки
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Подпись ссылки (если имеется)
        /// </summary>
        [JsonProperty("caption")]
        public string Caption { get; set; }

        /// <summary>
        /// Описание ссылки
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Изображение превью, объект фотографии (если имеется).
        /// </summary>
        [JsonProperty("photo")]
        public Photo Photo { get; set; }

        /// <summary>
        /// Информация о продукте (если имеется)
        /// </summary>
        [JsonProperty("product")]
        public Product Product { get; set; }

        /// <summary>
        /// Информация о кнопке для перехода (если имеется)
        /// </summary>
        [JsonProperty("button")]
        public string Button { get; set; }

    }
}
