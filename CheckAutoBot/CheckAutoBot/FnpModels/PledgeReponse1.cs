using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeResult1
    {
        [JsonProperty("list")]
        public List<PledgeListItem1> Pledges { get; set; }

        [JsonProperty("totalPages")]
        public string TotalPages { get; set; }

        [JsonProperty("currentPage")]
        public string CurrentPage { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
