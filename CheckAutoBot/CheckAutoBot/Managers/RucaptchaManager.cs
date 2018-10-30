using CheckAutoBot.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class RucaptchaManager
    {
        private readonly List<string> _warnings= new List<string>()
        {
            "ERROR_NO_SLOT_AVAILABLE",
            "ERROR_ZERO_CAPTCHA_FILESIZE",
            "ERROR_TOO_BIG_CAPTCHA_FILESIZE",
            "ERROR_WRONG_FILE_EXTENSION",
            "ERROR_IMAGE_TYPE_NOT_SUPPORTED",
            "ERROR_UPLOAD",
            "ERROR_CAPTCHAIMAGE_BLOCKED",
        };

        private readonly Rucaptcha _rucaptcha;

        public RucaptchaManager()
        {
            _rucaptcha = new Rucaptcha();
        }

        public CaptchaRequest SendImageCaptcha(string base64)
        {
            var captchaRequest = _rucaptcha.SendImageCaptcha(base64);

            if (!captchaRequest.State)
            {
                if (_warnings.Contains(captchaRequest.Id))
                    throw new InvalidOperationException(captchaRequest.Id);
                else
                    throw new Exception(captchaRequest.Id);
            }

           return captchaRequest;
        }

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl)
        {
            var captchaRequest = _rucaptcha.SendReCaptcha2(dataSiteKey, pageUrl);

            if (!captchaRequest.State)
            {
                if (_warnings.Contains(captchaRequest.Id))
                    throw new InvalidOperationException(captchaRequest.Id);
                else
                    throw new Exception(captchaRequest.Id);
            }

            return captchaRequest;
        }

        /// <summary>
        /// Выбрасывает исключение, если value это текст ошибки
        /// </summary>
        /// <param name="value"></param>
        public static void CheckCaptchaWord(string value)
        {
            if (!long.TryParse(value, out long _))
            {
                if (value.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                    throw new InvalidOperationException("ERROR_CAPTCHA_UNSOLVABLE");
                else
                    throw new Exception(value);
            }
        }
    }
}
