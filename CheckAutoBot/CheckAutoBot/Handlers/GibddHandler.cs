using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Handlers
{
    public class GibddHandler 
    {
        protected GibddManager _gibddManager { get; set; }
        protected RucaptchaManager _rucaptchaManager { get; set; }

        public GibddHandler(GibddManager gibddManager, RucaptchaManager rucaptchaManager)
        {
            _gibddManager = gibddManager;
            _rucaptchaManager = rucaptchaManager;
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _gibddManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);
            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }
    }
}
