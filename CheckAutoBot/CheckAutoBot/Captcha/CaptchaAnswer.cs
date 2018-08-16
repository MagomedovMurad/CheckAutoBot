using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Captcha
{
    public class CaptchaAnswer
    {
        [JsonProperty("status")]
        public bool State { get; set; }

        [JsonProperty("request")]
        public string Answer { get; set; }
    }
}
