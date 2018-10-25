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

        public PreGetResult(long captchaId, string sessionId)
        {
            CaptchaId = captchaId;
            SessionId = sessionId;
        }
        public long CaptchaId { get; set; }

        public string SessionId { get; set; }
    }
}
