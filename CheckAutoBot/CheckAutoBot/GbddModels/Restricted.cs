using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class Restricted
    {
        [JsonProperty("regname")]
        public string RegionName { get; set; }

        [JsonProperty("osnOgr")]
        public string RestrictedFoundations { get; set; }

        [JsonProperty("gid")]
        public string Gid { get; set; }

        [JsonProperty("tsyear")]
        public string VechicleYear { get; set; }

        [JsonProperty("tsVIN")]
        public string Vin { get; set; }

        [JsonProperty("codDL")]
        public int CodDL { get; set; }

        [JsonProperty("dateogr")]
        public string RestrictedDate { get; set; }

        [JsonProperty("ogrkod")]
        public int RestrictedTypeCode { get; set; }

        [JsonProperty("tsmodel")]
        public string TsModel { get; set; }

        [JsonProperty("tsKuzov")]
        public string TsBody { get; set; }

        [JsonProperty("codeTo")]
        public int Code { get; set; }

        [JsonProperty("dateadd")]
        public string DateAdd { get; set; }

        [JsonProperty("phone")]
        public string InitiatorPhone { get; set; }

        [JsonProperty("regid")]
        public string RegionId { get; set; }

        [JsonProperty("divtype")]
        public int InitiatorTypeCode { get; set; }

        [JsonProperty("divid")]
        public string DivId { get; set; }

        [JsonIgnore]
        public string InitiatorType
        {
            get
            {
                return initiatorTypes.GetValueOrDefault(InitiatorTypeCode, "Неизвестно");
            }
        }

        [JsonIgnore]
        public string RestrictedType
        {
            get
            {
                return restrictedTypes.GetValueOrDefault(RestrictedTypeCode, "Неизвестно");
            }
        }

        private Dictionary<int, string> initiatorTypes = new Dictionary<int, string>()
        {
            { 0, "Не предусмотренный код" },
            { 1, "Судебные органы" },
            { 2, "Судебный пристав" },
            { 3, "Таможенные органы"},
            { 4, "Органы социальной защиты"},
            { 5, "Нотариус"},
            { 6, "ОВД или иные правоохр. органы"},
            { 7, "ОВД или иные правоохр. органы (прочие)"}
        };
        private Dictionary<int, string> restrictedTypes = new Dictionary<int, string>()
        {
            { 0,"" },
            { 1, "Запрет на регистрационные действия" },
            { 2, "Запрет на снятие с учета" },
            { 3, "Запрет на регистрационные действия и прохождение ГТО" },
            { 4, "Утилизация (для транспорта не старше 5 лет)" },
            { 5, "Аннулирование" },
        };

    }
}
