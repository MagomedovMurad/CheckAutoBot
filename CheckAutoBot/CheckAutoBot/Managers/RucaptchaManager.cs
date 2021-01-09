using CheckAutoBot.Api;
using CheckAutoBot.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class RucaptchaManager
    {
        private readonly List<string> _inWarnings = new List<string>()
        {
            "ERROR_NO_SLOT_AVAILABLE",
            "ERROR_ZERO_CAPTCHA_FILESIZE",
            "ERROR_TOO_BIG_CAPTCHA_FILESIZE",
            "ERROR_WRONG_FILE_EXTENSION",
            "ERROR_IMAGE_TYPE_NOT_SUPPORTED",
            "ERROR_UPLOAD",
            "ERROR_CAPTCHAIMAGE_BLOCKED",
        };

        private static readonly List<string> _resWarnings = new List<string>()
        {
            "ERROR_CAPTCHA_UNSOLVABLE",
            "ERROR_BAD_DUPLICATES"
        };

        private readonly Rucaptcha _rucaptcha;

        public RucaptchaManager()
        {
            _rucaptcha = new Rucaptcha();
        }

        public string GetCaptchaResult(string captchaId)
        {
            return _rucaptcha.GetCapthaResult(captchaId).Answer;
        }

        public CaptchaRequest SendImageCaptcha(string base64, string pingback)
        {
            var captchaRequest = _rucaptcha.SendImageCaptcha(base64, pingback);

            if (!captchaRequest.State)
            {
                if (_inWarnings.Contains(captchaRequest.Id))
                    throw new InvalidOperationException(captchaRequest.Id);
                else
                    throw new Exception(captchaRequest.Id);
            }

           return captchaRequest;
        }

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl, string pingback)
        {
            var captchaRequest = _rucaptcha.SendReCaptcha2(dataSiteKey, pageUrl, pingback);

            if (!captchaRequest.State)
            {
                if (_inWarnings.Contains(captchaRequest.Id))
                    throw new InvalidOperationException(captchaRequest.Id);
                else
                    throw new Exception(captchaRequest.Id);
            }

            return captchaRequest;
        }
        public CaptchaRequest SendReCaptcha3(string dataSiteKey, string pageUrl, string pingback, int version, string action)
        {
            //var solver = new CaptchaSolver();
            //return solver.SendRecaptcha(pageUrl.Substring(8, pageUrl.Length - 8), action, dataSiteKey, pingback);
            var captchaRequest = _rucaptcha.SendReCaptcha3(dataSiteKey, pageUrl, pingback, version, action);

            if (!captchaRequest.State)
            {
                if (_inWarnings.Contains(captchaRequest.Id))
                    throw new InvalidOperationException(captchaRequest.Id);
                else
                    throw new Exception(captchaRequest.Id);
            }

            return captchaRequest;
        }

        public void SendReport(string id, bool isGood)
        {
            if (isGood)
                _rucaptcha.SendReportGood(id);
            else
                _rucaptcha.SendReportBad(id);
        }
        /// <summary>
        /// Выбрасывает исключение, если value это текст ошибки
        /// </summary>
        /// <param name="value"></param>
        public static void CheckCaptchaWord(string value)
        {
            if (value.StartsWith("ERROR"))
            {
                if (_resWarnings.Contains(value))
                    throw new InvalidOperationException(value);
                else
                    throw new Exception(value);
            }
        }
    }
}
