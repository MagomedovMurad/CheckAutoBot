using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class PreGetResult
    {
        public PreGetResult()
        {
        }

        public PreGetResult(string captchaId, string sessionId)
        {
            CaptchaId = captchaId;
            SessionId = sessionId;
        }

        public PreGetResult(string captchaId, string sessionId, string key)
        {
            CaptchaId = captchaId;
            SessionId = sessionId;
            Key = key;
        }
        public string CaptchaId { get; set; }

        public string SessionId { get; set; }

        public string Key { get; set; }
    }
}
