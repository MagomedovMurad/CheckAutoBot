using CheckAutoBot.Api;
using CheckAutoBot.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Controllers
{
    public interface ICaptchaController
    { 
    
    }

    public class CaptchaController: ICaptchaController
    {
        private readonly RucaptchaManager _rucaptchaManager;
        private readonly CaptchaSolver _captchaSolver;
        public CaptchaController(RucaptchaManager rucaptchaManager, CaptchaSolver captchaSolver)
        {
            _rucaptchaManager = rucaptchaManager;
            _captchaSolver = captchaSolver;
        }

        public string SendImageCaptcha(string base64, string pingback)
        {
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(base64, pingback);
            return captchaRequest.Id;
        }

        public string SendReCaptcha2(string dataSiteKey, string pageUrl, string pingback)
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha2(dataSiteKey, pageUrl, pingback);
            return captchaRequest.Id;
        }

        public string SendReCaptcha3(string dataSiteKey, string pageUrl, string pingback, string action)
        {
            var captchaRequest = _captchaSolver.SendRecaptcha(pageUrl, action, dataSiteKey, pingback);
            return captchaRequest.Id;
        }

        public string GetCaptchaResult(string captchaId)
        {
            return _rucaptchaManager.GetCaptchaResult(captchaId);
        }

        public void SendReport(string id, bool isGood)
        { 
        
        }
    }
}
