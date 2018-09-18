using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class UserRequestMessage
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор сообщения
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Тип запроса
        /// </summary>
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Дата и время получения запроса
        /// </summary>
        public DateTime Date { get; set; }
    }
}
