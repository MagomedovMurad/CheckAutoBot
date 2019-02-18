using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.GroupModels
{
    public class MessagesAllowedEvent
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [JsonProperty("user_id ")]
        public int UserId { get; set; }

        /// <summary>
        /// Какая-то строка (параметр переданный в messages.allowMessagesFromGroup)
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
