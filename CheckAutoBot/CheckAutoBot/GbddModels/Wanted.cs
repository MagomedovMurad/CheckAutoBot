using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class Wanted
    {
        [JsonProperty("w_rec")]
        public int Order { get; set; }

        [JsonProperty("w_reg_inic")]
        public string RegionIniciator { get; set; }

        [JsonProperty("w_model")]
        public string VechicleModel { get; set; }

        [JsonProperty("w_data_pu")]
        public string Date { get; set; }

        [JsonProperty("w_god_vyp")]
        public string VechicleYear { get; set; }

        [JsonProperty("w_un_gic")]
        public string w_un_gic { get; set; }
    }
}
