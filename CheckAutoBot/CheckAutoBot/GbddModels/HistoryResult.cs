using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class HistoryResult
    {
        /// <summary>
        /// Периоды владения автомобилем
        /// </summary>
        [JsonProperty("ownershipPeriods")]
        public List<OwnershipPeriod> OwnershipPeriods { get; set; }

        /// <summary>
        /// Данные о ПТС автомобиля
        /// </summary>
        [JsonProperty("vehiclePassport")]
        public VehiclePassport VehiclePassport { get; set; }

        /// <summary>
        /// Данные об автомобиле
        /// </summary>
        [JsonProperty("vehicle")]
        public Vechicle Vehicle { get; set; }
    }
}
