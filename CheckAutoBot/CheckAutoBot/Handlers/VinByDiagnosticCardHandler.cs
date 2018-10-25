using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class VinByDiagnosticCardHandler : IHandler
    {
        IActorRef _requestHandler;
        private DbQueryExecutor _queryExecutor;
        private EaistoManager _eaistoManager { get; set; }
        private RucaptchaManager _rucaptchaManager { get; set; }

        public ActionType SupportedActionType => ActionType.VinByDiagnosticCard;

        public VinByDiagnosticCardHandler(IActorRef requestHandler, DbQueryExecutor queryExecutor)
        {
            _requestHandler = requestHandler;
            _queryExecutor = queryExecutor;
            _rucaptchaManager = new RucaptchaManager();
            _eaistoManager = new EaistoManager();
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, IEnumerable<CacheItem> cacheItems)
        {
            var auto = requestObject as Auto;

            var diagnosticCardCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.VinByDiagnosticCard);
            int requestId = diagnosticCardCacheItem.RequestId;
            var targetActionType = diagnosticCardCacheItem.TargetActionType;

            var diagnosticCard = _eaistoManager.GetDiagnosticCard(diagnosticCardCacheItem.CaptchaWord, null, diagnosticCardCacheItem.SessionId, licensePlate: auto.LicensePlate);

            List<StartActionMessage> messages = new List<StartActionMessage>();

            if (diagnosticCard == null)
            {
                messages.Add(
                    new StartActionMessage()
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.PolicyNumber,
                        TargetActionType = targetActionType
                    });
                messages.Add(
                    new StartActionMessage()
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.OsagoVechicle,
                        TargetActionType = targetActionType
                    });
            }
            else
            {
                messages.Add(
                    new StartActionMessage()
                    {
                        RequestId = requestId,
                        CurrentActionType = targetActionType,
                        TargetActionType = targetActionType
                    });

                var request = _queryExecutor. GetUserRequest(requestId).Result;
                _queryExecutor.UpdateVinCode(request.RequestObject.Id, diagnosticCard.Vin);
            }

            messages.ForEach(m => _requestHandler.Tell(m));

            return null;
        }


    }
}
