using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Handlers
{
    public class GetOsagoVechicleForVinHandler : GetPolicyNumberForVinHandler
    {
        public GetOsagoVechicleForVinHandler(RucaptchaManager rucaptchaManager, RsaManager rsaManager): base(rucaptchaManager, rsaManager)
        {
        }

        public override ActionType SupportedActionType => ActionType.OsagoVechicle;

        public override PreGetResult PreGet()
        {
            var captchaRequestOV = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl, Rucaptcha.LpPingbackUrl);
           return new PreGetResult(captchaRequestOV.Id, null);
        }
    }
}
