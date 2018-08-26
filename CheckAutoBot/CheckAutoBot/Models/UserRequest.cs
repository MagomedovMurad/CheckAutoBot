using CheckAutoBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Models
{
    public class UserRequest
    {
        /// <summary>
        /// Идентификатор запроса
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор запроса
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор сообщения
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Тип запроса
        /// </summary>
        public UserRequestType Type { get; set; }

        /// <summary>
        /// Входные данные для запроса (гос. номер, вин код, ФИО)
        /// </summary>
        public string InputData { get; set; }

        /// <summary>
        /// Дата получения запроса
        /// </summary>
        public DateTimeOffset Date { get; set; }
    }
}
