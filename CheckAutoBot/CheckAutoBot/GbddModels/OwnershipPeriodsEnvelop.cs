using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.GbddModels
{
    public class OwnershipPeriodsEnvelop
    {
        /// <summary>
        /// Периоды владения автомобилем
        /// </summary>
        [JsonProperty("ownershipPeriods")]
        public List<OwnershipPeriod> OwnershipPeriods { get; set; }
    }
}
