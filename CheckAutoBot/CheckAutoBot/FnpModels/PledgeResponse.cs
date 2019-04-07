using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class PledgeResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("data")]
        public List<Pledge> Data { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
       
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

    }
}
