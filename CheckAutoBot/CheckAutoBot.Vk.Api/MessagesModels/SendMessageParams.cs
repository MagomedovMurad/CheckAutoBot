using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.MessagesModels
{
    public class SendMessageParams
    {
        /// <summary>
        /// Идентификатор назначения 
        /// </summary>
        public long PeerId { get; set; }

        /// <summary>
        /// Tекст личного сообщения. Обязательный параметр, если не задан параметр attachment
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Медиавложения к личному сообщению, перечисленные через запятую. Обязательный параметр, если не задан параметр Message
        /// 
        public string Attachments { get; set; }

        /// <summary>
        /// Объект, описывающий клавиатуру для бота
        /// </summary>
        public Keyboard Keyboard { get; set; }

        /// <summary>
        /// Ключ доступа
        /// </summary>
        public string AccessToken { get; set; }
    }
}
