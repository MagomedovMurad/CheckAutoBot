using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class DtpResult
    {
        [JsonProperty("errorDescription")]
        public string ErrorDescription { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("Accidents")]
        public List<Accident> Accidents { get; set; }
    }
}
