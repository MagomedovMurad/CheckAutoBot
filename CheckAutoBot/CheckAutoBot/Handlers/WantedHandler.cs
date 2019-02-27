using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class WantedHandler : GibddHandler, IHttpHandler
    {
        public WantedHandler(GibddManager gibddManager, 
                             RucaptchaManager rucaptchaManager) : base(gibddManager, rucaptchaManager)
        {
        }

        public ActionType SupportedActionType => ActionType.Wanted;

        public PreGetResult PreGet()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.RequestPingbackUrl, 3, "check_auto_wanted");
            return new PreGetResult(captchaRequest.Id, null);
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, string captchaWord, string sessionId)
        {
            var auto = requestObject as Auto;

            //var wantedCacheItem = cacheItems.First(x => x.ActionType == ActionType.Wanted);
            var wantedResponse = _gibddManager.GetWanted(auto.Vin, captchaWord, sessionId);
            return GenerateResponse(wantedResponse);

        }

        private Dictionary<string, byte[]> GenerateResponse(WantedResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "✅ В базе ГИБДД не найдены сведения о розыске транспортного средства";
                messages.Add(text, null);
            }
            else
            {
                for (int i = 0; i < result.Wanteds?.Count; i++)
                {
                    var text = WantedToMessageText(result.Wanteds[i], i+1);
                    messages.Add(text, null);
                }
            }

            return messages;
        }

        private string WantedToMessageText(Wanted wanted, int number)
        {
            return $"🕵 {number}. Информация о постановке в розыск{Environment.NewLine}" +
                   $"Марка, модель: {wanted.VechicleModel}{Environment.NewLine}" +
                   $"Год выпуска: {wanted.VechicleYear}{Environment.NewLine}" +
                   $"Дата объявления в розыск: {wanted.Date}{Environment.NewLine}" +
                   $"Регион инициатора розыска: {wanted.RegionIniciator}";
        }


    }
}
