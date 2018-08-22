using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.MessagesModels
{
    public class PrivateMessage
    {
        /// <summary>
        /// Идентификатор сообщения
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Время отправки в Unixtime
        /// </summary>
        //public int Date { get; set; }

        /// <summary>
        /// Идентификатор назначения
        /// </summary>
        [JsonProperty("peer_id")]
        public int PeerId { get; set; }

        /// <summary>
        /// Идентификатор отправителя
        /// </summary>
        [JsonProperty("from_id")]
        public int FromId { get; set; }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Идентификатор, используемый при отправке сообщения. Возвращается только для исходящих сообщений
        /// </summary>
        [JsonProperty("random_id")]
        public int RandomId { get; set; }

        /// <summary>
        /// Медиавложения сообщения (фотографии, ссылки и т.п.)
        /// </summary>
        //public string[] Attachments { get; set; }

        /// <summary>
        /// True, если сообщение помечено как важное
        /// </summary>
        [JsonProperty("important")]
        public bool Important { get; set; }

        /// <summary>
        /// Информация о местоположении
        /// </summary>
        //public object Geo { get; set; }

        /// <summary>
        /// Сервисное поле для сообщений ботам (полезная нагрузка)
        /// </summary>
        //public string Payload { get; set; }

        /// <summary>
        /// Массив пересланных сообщений (если есть)
        /// </summary>
        //public PrivateMessage[] FwdMessages { get; set; }

        /// <summary>
        /// Информация о сервисном действии с чатом
        /// </summary>
        //public object Action { get; set; }
    }
}
