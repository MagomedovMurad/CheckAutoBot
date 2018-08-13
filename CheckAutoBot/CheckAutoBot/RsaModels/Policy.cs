using CheckAutoBot.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.RsaModels
{
    public class Policy
    {
        /// <summary>
        /// Наименование компании, которая выдала полис осаго
        /// </summary>
        [JsonProperty("insCompanyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Номер полиса осаго
        /// </summary>
        [JsonProperty("policyBsoNumber")]
        public string Number { get; set; }

        /// <summary>
        /// Серия полиса осаго
        /// </summary>
        [JsonProperty("policyBsoSerial")]
        public string Serial { get; set; }

        /// <summary>
        /// Значение показывающее, является ли страховка ограниченной
        /// </summary>
        [JsonConverter(typeof(BoolConverter))]
        [JsonProperty("policyIsRestrict")]
        public bool IsRestrict { get; set; }

        /// <summary>
        /// Уникальный идентификатор полиса осаго
        /// </summary>
        [JsonProperty("policyUnqId")]
        public string UnqId { get; set; }
    }
}
