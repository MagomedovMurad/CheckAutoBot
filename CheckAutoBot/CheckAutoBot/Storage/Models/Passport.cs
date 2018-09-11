using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class Passport: RequestObject
    {
        /// <summary>
        /// Серия паспорта
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Номер паспорта
        /// </summary>
        public string Number { get; set; }
    }
}
