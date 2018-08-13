using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.RsaModels
{
    public class PolicyResponse
    {
        /// <summary>
        /// Номер кузова автомобиля
        /// </summary>
        [JsonProperty("bodyNumber")]
        public string BodyNumber { get; set; }

        /// <summary>
        /// Номер насси автомобиля
        /// </summary>
        [JsonProperty("chassisNumber")]
        public string ChassisNumber { get; set; }

        /// <summary>
        /// Номер государственного регистрационного знака автомобиля
        /// </summary>
        [JsonProperty("licensePlate")]
        public string LicensePlate { get; set; }

        /// <summary>
        /// Вин код автомобиля
        /// </summary>
        [JsonProperty("vin")]
        public string Vin { get; set; }

        /// <summary>
        /// Уникальный идентификатор полиса осаго
        /// </summary>
        [JsonProperty("policyUnqId")]
        public string PolicyUnqId { get; set; }

        /// <summary>
        /// Список полисов осаго
        /// </summary>
        [JsonProperty("policyResponseUIItems")]
        public List<Policy> Policies { get; set; }

        /// <summary>
        /// Значение, показывающее правильно ли введена каптча
        /// </summary>
        [JsonProperty("validCaptcha")]
        public bool ValidCaptcha { get; set; }

        /// <summary>
        /// Ошибка
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Идентификатор ошибки
        /// </summary>
        [JsonProperty("errorId")]
        public int ErrorId { get; set; }

        /// <summary>
        /// Предупреждение
        /// </summary>
        [JsonProperty("warningMessage")]
        public string WarningMessage { get; set; }
        
    }

}
