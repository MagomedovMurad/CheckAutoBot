using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Exceptions
{
    public class InvalidCaptchaException: InvalidOperationException
    {
        public InvalidCaptchaException(string captchaWord)
        {
            CaptchaWord = captchaWord;
        }

        public string CaptchaWord { get; set; }
    }
}
