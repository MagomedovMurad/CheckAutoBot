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

        public CaptchaResponseMessage(long captchaId, string value)
        {
            CaptchaId = captchaId;
            Value = value;
        }
        
        #endregion Ctor

        [JsonProperty()]
        public long CaptchaId { get; set; }

        [JsonProperty()]
        public string Value { get; set; }
    }
}
