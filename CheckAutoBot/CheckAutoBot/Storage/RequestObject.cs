using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class RequestObject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Дата добавления
        /// </summary>
        public DateTime Date { get; set; }
    }
}
