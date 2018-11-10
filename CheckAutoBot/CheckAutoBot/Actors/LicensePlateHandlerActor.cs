using Akka.Actor;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Handlers;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class LicensePlateHandlerActor: ReceiveActor
    {
        private List<CaptchaCacheItem> _captchaCacheItems; 
        private RsaManager _rsaManager;
        private RucaptchaManager _rucaptchaManager;
        private EaistoManager _eaistoManager;

        private List<IHandlerVinFinder> _handlers;

        public LicensePlateHandlerActor()
        {
            Receive<StartVinSearchingMessage>(x => StartVinSearch(x));
            Receive<CaptchaResponseMessage>(x => CaptchaResponseMessageHandler(x));
        }

        private void InitHandlers()
        {
            _handlers = new List<IHandlerVinFinder>()
            {
                new VinByDiagnosticCardHandler()
            };
        }

        private void StartVinSearch(StartVinSearchingMessage message)
        {
            var preGetResult = PreGetDiagnosticCard();
            AddCaptchaCacheItem(message.RequestObjectId, ActionType.DiagnosticCardForVin, preGetResult.CaptchaId, preGetResult.SessionId);
        }

        private void CaptchaResponseMessageHandler(CaptchaResponseMessage message)
        {
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

        }

        public PreGetResult PreGetDiagnosticCard()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }

        public void GetDiagnosticCard(string captcha, string sessionId, string licensePlate)
        {
            var diagnosticCard = _eaistoManager.GetDiagnosticCard(captcha, null, sessionId, licensePlate: licensePlate);
        }

        private void AddCaptchaCacheItem(int id, ActionType actionType, string captchaId, string sessionId)
        {
            _captchaCacheItems.Add(
                new CaptchaCacheItem()
                {
                    Id = id,
                    ActionType = actionType,
                    CaptchaId = captchaId,
                    SessionId = sessionId,
                    Date = DateTime.Now
                });
        }
    }
}
