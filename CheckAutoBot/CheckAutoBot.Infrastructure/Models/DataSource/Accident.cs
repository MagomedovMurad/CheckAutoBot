using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class DtpAccident
    {
        /// <summary>
        /// Дата и время происшествия
        /// </summary>
        public string AccidentDateTime { get; set; }

        /// <summary>
        /// Модель автомобиля
        /// </summary>
        public string VehicleModel { get; set; }

        /// <summary>
        /// Соостояние автомобиля
        /// </summary>
        public string VehicleDamageState { get; set; }

        /// <summary>
        /// Регион происшествия
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// Номер инцидента
        /// </summary>
        public string AccidentNumber { get; set; }

        /// <summary>
        /// Тип происшествия
        /// </summary>
        public string AccidentType { get; set; }

        /// <summary>
        /// Марка автомобиля
        /// </summary>
        public string VehicleMark { get; set; }

        /// <summary>
        /// Поврежденные участки
        /// </summary>
        public string[] DamagePoints { get; set; }

        /// <summary>
        /// Год выпуска автомобиля
        /// </summary>
        public string VehicleYear { get; set; }

        public byte[] Picture { get; set; }
    }
}
