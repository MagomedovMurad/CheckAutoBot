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
        public string TsYear { get; set; }

        [JsonProperty("tsVIN")]
        public string Vin { get; set; }

        [JsonProperty("tsVIN")]
        public int IntVin { get; set; }

        [JsonProperty("dateogr")]
        public DateTimeOffset RestrictedDate { get; set; }

        [JsonProperty("ogrkod")]
        public int RestrictedCode { get; set; }

        [JsonProperty("tsmodel")]
        public string TsModel { get; set; }

        [JsonProperty("tsKuzov")]
        public string TsBody { get; set; }

        [JsonProperty("codeTo")]
        public int Code { get; set; }

        [JsonProperty("dateadd")]
        public DateTimeOffset DateAdd { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("regid")]
        public string RegionId { get; set; }

        [JsonProperty("divtype")]
        public string DivType { get; set; }

        [JsonProperty("divid")]
        public string DivId { get; set; }

    }
}
