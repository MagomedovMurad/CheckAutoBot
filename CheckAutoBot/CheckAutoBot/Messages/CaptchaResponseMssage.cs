using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class CaptchaResponseMssage
    {
        #region Ctor

        public CaptchaResponseMssage()
        {

        }

        public CaptchaResponseMssage(string captchaId, string value)
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
