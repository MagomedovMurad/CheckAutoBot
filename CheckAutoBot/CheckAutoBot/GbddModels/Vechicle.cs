using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class Vechicle
    {
        [JsonProperty("engineVolume")]
        public string EngineVolume { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("bodyNumber")]
        public string BodyNumber { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("engineNumber")]
        public string EngineNumber { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("type")]
        public int TypeCode { get; set; }

        [JsonIgnore]
        public string TypeName
        {
            get
            {
               return vechicleTypes.GetValueOrDefault(TypeCode, "Неизвестно");
            }
        }

        [JsonProperty("powerHp")]
        public string PowerHp { get; set; }

        [JsonProperty("powerKwt")]
        public string PowerKwt { get; set; }

        private Dictionary<int, string> vechicleTypes = new Dictionary<int, string>()
        {
            { 1, "Грузовые автомобили бортовые"},
            { 2, "Грузовые автомобили шасси" },
            { 3, "Грузовые автомобили фургоны" },
            { 4, "Грузовые автомобили тягачи седельные" },
            { 5, "Грузовые автомобили самосвалы" },
            { 6, "Грузовые автомобили рефрижераторы" },
            { 7, "Грузовые автомобили цистерны" },
            { 8, "Грузовые автомобили с гидроманипулятором" },
            { 9, "Грузовые автомобили прочие" },
            { 21, "Легковые автомобили универсал" },
            { 22, "Легковые автомобили хэтчбек" },
            { 23, "Легковые автомобили седан" },
            { 24, "Легковые автомобили лимузин" },
            { 25, "Легковые автомобили купе" },
            { 26, "Легковые автомобили кабриолет" },
            { 27, "Легковые автомобили фаэтон" },
            { 28, "Легковые автомобили пикап" },
            { 29, "Легковые автомобили прочие" },
            { 71, "Мотоциклы" },
            { 81, "Прицепы к легковым автомобилям" },
            { 39, "Иной" }
        };
    }


}
