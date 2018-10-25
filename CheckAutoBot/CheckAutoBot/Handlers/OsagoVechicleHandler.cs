using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class OsagoVechicleHandler : PolicyNumberHandler
    {
        public OsagoVechicleHandler(RsaManager rsaManager,
                                    RucaptchaManager rucaptchaManager,
                                    IActorRef requestHandler, 
                                    DbQueryExecutor queryExecutor) : base(rsaManager, rucaptchaManager, requestHandler, queryExecutor)
        {
        }

        public override ActionType SupportedActionType => ActionType.OsagoVechicle;

        public override PreGetResult PreGet()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl);
            return new PreGetResult(captchaRequest.Id, null);
        }
    }
}
