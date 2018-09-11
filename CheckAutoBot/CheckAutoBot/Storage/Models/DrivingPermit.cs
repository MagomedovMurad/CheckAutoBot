using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class DrivingPermit: RequestObject
    {
        /// <summary>
        /// Серия ВУ
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Номер ВУ
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Дата выдачи ВУ
        /// </summary>
        public string IssueDate { get; set; }
    }
}
