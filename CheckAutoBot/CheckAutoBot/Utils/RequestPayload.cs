using CheckAutoBot.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class RequestPayload
    {
        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Переопределен! Конвертирует в Json.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
