using CheckAutoBot.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class RucaptchaManager
    {
        private readonly Rucaptcha _rucaptcha;

        public RucaptchaManager()
        {
            _rucaptcha = new Rucaptcha();
        }

        public CaptchaRequest SendImageCaptcha(string base64)
        {
            return _rucaptcha.SendImageCaptcha(base64);
        }

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl)
        {
            return _rucaptcha.SendReCaptcha2(dataSiteKey, pageUrl);
        }
    }
}
