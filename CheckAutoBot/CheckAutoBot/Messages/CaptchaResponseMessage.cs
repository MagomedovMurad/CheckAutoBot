using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class CaptchaResponseMessage
    {
        #region Ctor

        public CaptchaResponseMessage()
        {

        }

        public CaptchaResponseMessage(string captchaId, string value)
        {
            CaptchaId = captchaId;
            Value = value;
        }
        
        #endregion Ctor

        [JsonProperty()]
        public string CaptchaId { get; set; }

        [JsonProperty()]
        public string Value { get; set; }
    }
}
