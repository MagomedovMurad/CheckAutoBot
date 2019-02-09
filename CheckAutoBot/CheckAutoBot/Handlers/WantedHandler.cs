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
    public class WantedHandler : GibddHandler, IHandler
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

        public Dictionary<string, byte[]> Get(RequestObject requestObject, CaptchaCacheItem cacheItem)
        {
            var auto = requestObject as Auto;

            //var wantedCacheItem = cacheItems.First(x => x.ActionType == ActionType.Wanted);
            var wantedResponse = _gibddManager.GetWanted(auto.Vin, cacheItem.CaptchaWord, cacheItem.SessionId);
            return GenerateResponse(wantedResponse);

        }

        private Dictionary<string, byte[]> GenerateResponse(WantedResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о розыске транспортного средства";
                messages.Add(text, null);
            }
            else
            {
                foreach (var wanted in result.Wanteds)
                {
                    var text = WantedToMessageText(wanted);
                    messages.Add(text, null);
                }
            }

            return messages;
        }

        private string WantedToMessageText(Wanted wanted)
        {
            return $"Информация о постановке в розыск{Environment.NewLine}" +
                   $"Марка, модель: {wanted.VechicleModel}{Environment.NewLine}" +
                   $"Год выпуска: {wanted.VechicleYear}{Environment.NewLine}" +
                   $"Дата объявления в розыск: {wanted.Date}{Environment.NewLine}" +
                   $"Регион инициатора розыска: {wanted.RegionIniciator}";
        }


    }
}
