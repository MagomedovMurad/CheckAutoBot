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
using CheckAutoBot.RsaModels;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class PolicyNumberHandler : IHandler
    {
        protected RucaptchaManager _rucaptchaManager;
        protected RsaManager _rsaManager;
        protected IActorRef _requestHandler;
        protected DbQueryExecutor _queryExecutor;


        public PolicyNumberHandler(RsaManager rsaManager, RucaptchaManager rucaptchaManager, IActorRef requestHandler, DbQueryExecutor queryExecutor)
        {
            _rucaptchaManager = rucaptchaManager;
            _rsaManager = rsaManager;
            _requestHandler = requestHandler;
            _queryExecutor = queryExecutor;
        }


        public virtual ActionType SupportedActionType => ActionType.PolicyNumber;

        public virtual PreGetResult PreGet()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl);
            return new PreGetResult(captchaRequest.Id, null);
        }


        public Dictionary<string, byte[]> Get(RequestObject requestObject, IEnumerable<CaptchaCacheItem> cacheItems)
        {
            var auto = requestObject as Auto;

            var policyNumberCacheItem = cacheItems.First(x => x.ActionType == ActionType.PolicyNumber);
            var requestId = policyNumberCacheItem.Id;
            var targetActionType = policyNumberCacheItem.TargetActionType;

            var policyResponse = _rsaManager.GetPolicy(policyNumberCacheItem.CaptchaWord, DateTime.Now, lp: auto.LicensePlate);

            VechicleResponse vechicleResponse = null;

            if (policyResponse != null)
            {
                var policy = policyResponse.Policies.FirstOrDefault();
                var policyInfoCacheItem = cacheItems.First(x => x.ActionType == ActionType.OsagoVechicle);

                vechicleResponse = _rsaManager.GetVechicleInfo(policy.Serial, policy.Number, DateTime.Now, policyInfoCacheItem.CaptchaWord);
            }

            if (vechicleResponse != null)
            {
                var message = new StartActionMessage()
                {
                    RequestId = requestId,
                    CurrentActionType = targetActionType,
                    TargetActionType = targetActionType
                };

                var request = _queryExecutor.GetUserRequest(requestId).Result;
                _queryExecutor.UpdateVinCode(request.RequestObject.Id, vechicleResponse.Vin);
                _requestHandler.Tell(message);

                return null;
            }

            return new Dictionary<string, byte[]>()
            {
                { StaticResources.VinNotFoundError, null }
            };
        }
    }
}
