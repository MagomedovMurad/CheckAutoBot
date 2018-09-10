using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class Auto: RequestObject
    {
        /// <summary>
        /// Гос. номер автомобиля
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Вин код автомобиля
        /// </summary>
        public string Vin { get; set; }
    }
}
