using CheckAutoBot.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.RsaModels
{
    public class VechicleResponse
    {
        /// <summary>
        /// Номер кузова автомобиля
        /// </summary>
        [JsonProperty("bodyNumber")]
        public string BodyNumber { get; set; }

        /// <summary>
        /// Номер шасси автомобиля
        /// </summary>
        [JsonProperty("chassisNumber")]
        public string ChassisNumber { get; set; }

        /// <summary>
        /// Государственный регистрационный знак автомобиля
        /// </summary>
        [JsonProperty("licensePlate")]
        public string LicensePlate { get; set; }

        /// <summary>
        /// Наименование компании, выдавшей полис ОСАГО
        /// </summary>
        [JsonProperty("insurerName")]
        public string InsurerName { get; set; }


        /// <summary>
        /// Статус полиса ОСАГО (действует/недействует)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("policyStatus")]
        public PolicyStatus? PolicyStatus { get; set; }

        /// <summary>
        /// Вин код автомобиля
        /// </summary>
        [JsonProperty("vin")]
        public string Vin { get; set; }

        /// <summary>
        /// Значение, показывающее правильно ли введена каптча
        /// </summary>
        [JsonProperty("validCaptcha")]
        public bool ValidCaptcha { get; set; }

        /// <summary>
        /// Предупреждение
        /// </summary>
        [JsonProperty("warningMessage")]
        public string WarningMessage { get; set; }

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
    }
}
