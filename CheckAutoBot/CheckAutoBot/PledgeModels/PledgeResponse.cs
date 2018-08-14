using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeResponse
    {
        [JsonProperty("list")]
        public List<Pledge> Pledges { get; set; }

        [JsonProperty("totalPages")]
        public string TotalPages { get; set; }

        [JsonProperty("currentPage")]
        public string CurrentPage { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
