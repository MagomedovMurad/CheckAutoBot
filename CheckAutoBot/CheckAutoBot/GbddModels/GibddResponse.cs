using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class GibddResponse<T>
    {
        /// <summary>
        /// Результат расчета
        /// </summary>
        [JsonProperty("RequestResult")]
        public T RequestResult { get; set; }

        /// <summary>
        /// Вин код автомобиля
        /// </summary>
        [JsonProperty("vin")]
        public string Vin { get; set; }

        /// <summary>
        /// Статус 
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
