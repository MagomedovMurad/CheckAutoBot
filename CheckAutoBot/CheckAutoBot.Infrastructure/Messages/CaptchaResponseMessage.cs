using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Messages
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

        public string CaptchaId { get; set; }

        public string Value { get; set; }
    }
}
