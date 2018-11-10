using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class VinByDiagnosticCardHandler : IHandlerVinFinder
    {
        private EaistoManager _eaistoManager { get; set; }
        private RucaptchaManager _rucaptchaManager { get; set; }

        public ActionType SupportedActionType => ActionType.DiagnosticCardForVin;

        public VinByDiagnosticCardHandler()
        {
            _rucaptchaManager = new RucaptchaManager();
            _eaistoManager = new EaistoManager();
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }

        public string Get(string licensePlate, IEnumerable<CaptchaCacheItem> cacheItems)
        {
            var diagnosticCardCacheItem = cacheItems.First(x => x.ActionType == ActionType.DiagnosticCardForVin);
            var diagnosticCard = _eaistoManager.GetDiagnosticCard(diagnosticCardCacheItem.CaptchaWord, null, diagnosticCardCacheItem.SessionId, licensePlate: licensePlate);

            if (diagnosticCard == null)
                return null;

            return diagnosticCard.Vin;
        }


    }
}
