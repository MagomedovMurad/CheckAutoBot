using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public class PledgeReponse
    {
        [JsonProperty("list")]
        public List<PledgeListItem> Pledges { get; set; }

        [JsonProperty("totalPages")]
        public string TotalPages { get; set; }

        [JsonProperty("currentPage")]
        public string CurrentPage { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
