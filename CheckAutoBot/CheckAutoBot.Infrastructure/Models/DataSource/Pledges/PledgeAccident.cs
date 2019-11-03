using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class PledgeAccident
    {
        /// <summary>
        /// Номер/идентификатор залогового уведомления
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Дата регистрации уведомления о залоге
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Залогодатели
        /// </summary>
        public List<Pledgor> Pledgors { get; set; }

        /// <summary>
        /// Залогодержатели
        /// </summary>
        public List<Pledgee> Pledgees { get; set; }
    }
}
