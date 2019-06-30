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
    public class GetPolicyNumberForVinHandler : IVinFinderHandler
    {
        protected RucaptchaManager _rucaptchaManager;
        protected RsaManager _rsaManager;

        public GetPolicyNumberForVinHandler(RucaptchaManager rucaptchaManager, RsaManager rsaManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _rsaManager = rsaManager;
        }
        public virtual ActionType SupportedActionType => ActionType.PolicyNumber;

        public string Get(string licensePlate, IEnumerable<ActionCacheItem> captchas)
        {
            var policyNumberCacheItem = captchas.First(x => x.ActionType == ActionType.PolicyNumber);
            var osagoVechicleCacheItem = captchas.First(x => x.ActionType == ActionType.OsagoVechicle);

            string vin = null;

            var policyResponse = _rsaManager.GetPolicy(policyNumberCacheItem.CaptchaWord, DateTime.Now, lp: licensePlate);
            if (policyResponse != null)
            {
                var policy = policyResponse.Policies.FirstOrDefault();
                var vechicleResponse = _rsaManager.GetVechicleInfo(policy.Serial, policy.Number, DateTime.Now, osagoVechicleCacheItem.CaptchaWord);
                vin = vechicleResponse.Vin;
            }

            return vin;
        }

        public virtual PreGetResult PreGet()
        {
            var captchaRequestPN = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl, Rucaptcha.LpPingbackUrl);
            return new PreGetResult(captchaRequestPN.Id, null);
        }
    }
}
