using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class Accident
    {
        /// <summary>
        /// Дата и время происшествия
        /// </summary>
        [JsonProperty("AccidentDateTime")]
        public string AccidentDateTime { get; set; }

        /// <summary>
        /// Модель автомобиля
        /// </summary>
        [JsonProperty("VehicleModel")]
        public string VehicleModel { get; set; }

        /// <summary>
        /// Соостояние автомобиля
        /// </summary>
        [JsonProperty("VehicleDamageState")]
        public string VehicleDamageState { get; set; }

        /// <summary>
        /// Регион происшествия
        /// </summary>
        [JsonProperty("RegionName")]
        public string RegionName { get; set; }

        /// <summary>
        /// Номер инцидента
        /// </summary>
        [JsonProperty("AccidentNumber")]
        public string AccidentNumber { get; set; }

        /// <summary>
        /// Тип происшествия
        /// </summary>
        [JsonProperty("AccidentType")]
        public string AccidentType { get; set; }

        /// <summary>
        /// Марка автомобиля
        /// </summary>
        [JsonProperty("VehicleMark")]
        public string VehicleMark { get; set; }

        /// <summary>
        /// Поврежденные участки
        /// </summary>
        [JsonProperty("DamagePoints")]
        public string[] DamagePoints { get; set; }

        /// <summary>
        /// Год выпуска автомобиля
        /// </summary>
        [JsonProperty("VehicleYear")]
        public string VehicleYear { get; set; }
    }
}
