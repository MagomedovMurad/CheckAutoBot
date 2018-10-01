using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    [JsonObject(id: "RequestResult")]
    public class HistoryResult
    {
        /// <summary>
        /// Конверт для периодов владения автомобилем
        /// </summary>
        [JsonProperty("ownershipPeriods")]
        public OwnershipPeriodsEnvelop OwnershipPeriodsEnvelop { get; set; }

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
