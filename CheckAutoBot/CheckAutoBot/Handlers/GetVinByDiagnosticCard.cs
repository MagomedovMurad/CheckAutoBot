using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Handlers
{
    public class GetVinByDiagnosticCard : IVinFinderHandler
    {
        EaistoManager _eaistoManager;
        RucaptchaManager _rucaptchaManager;

        public GetVinByDiagnosticCard(EaistoManager eaistoManager, RucaptchaManager rucaptchaManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _eaistoManager = eaistoManager;
        }

        public ActionType SupportedActionType => ActionType.DiagnosticCard;

        public string Get(string licensePlate, IEnumerable<ActionCacheItem> captchas)
        {
            var dkCacheItem = captchas.FirstOrDefault(x => x.ActionType == ActionType.DiagnosticCard);

            var diagnosticCard = _eaistoManager.GetLastDiagnosticCard(dkCacheItem.CaptchaWord, null, dkCacheItem.SessionId, licensePlate: licensePlate);
            if (diagnosticCard == null)
                return null;

            return diagnosticCard.Vin;
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.LpPingbackUrl);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }
    }
}
