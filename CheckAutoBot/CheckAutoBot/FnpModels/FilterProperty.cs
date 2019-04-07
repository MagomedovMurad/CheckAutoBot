using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public class FilterProperty
    {
        
        [JsonProperty("vehicleProperty")]
        public VechicleProperty VechicleProperty { get; set; }
    }
}
