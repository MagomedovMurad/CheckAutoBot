using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Models.Captcha
{
    public class CaptchaRequestData
    {
        public CaptchaRequestData(string captchaId, string sessionId, string key)
        {
            CaptchaId = captchaId;
            SessionId = sessionId;
            Key = key;
        }

        public string CaptchaId { get; }

        public string SessionId { get; }

        public string Key { get; }

        public string Value { get; set; }
    }
}
