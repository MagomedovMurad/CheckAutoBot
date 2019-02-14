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
    public class RestrictedHandler : GibddHandler, IHandler
    {
        public RestrictedHandler(GibddManager gibddManager,
                                 RucaptchaManager rucaptchaManager) : base(gibddManager, rucaptchaManager)
        {
        }

        public ActionType SupportedActionType => ActionType.Restricted;

        public PreGetResult PreGet()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.RequestPingbackUrl, 3, "check_auto_restricted");
            return new PreGetResult(captchaRequest.Id, null);
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, CaptchaCacheItem cacheItem)
        {
            var auto = requestObject as Auto;

            //var restrictedCacheItem = cacheItems.First(x => x.ActionType == ActionType.Restricted);
            var restrictedResult = _gibddManager.GetRestrictions(auto.Vin, cacheItem.CaptchaWord, cacheItem.SessionId);
            return GenerateResponse(restrictedResult);
        }

        private Dictionary<string, byte[]> GenerateResponse(RestrictedResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о наложении ограничений";
                messages.Add(text, null);
            }
            else
            {
                for (int i = 0; i < result.Restricteds?.Count; i++)
                {
                    var text = RestrictedToMessageText(result.Restricteds[i], i+1);
                    messages.Add(text, null);
                }
            }

            return messages;
        }

        private string RestrictedToMessageText(Restricted restricted, int number)
        {
            return $"{number}. Информация о наложении ограничения{Environment.NewLine}" +
                   $"Марка, модель ТС: {restricted.TsModel}{Environment.NewLine}" +
                   $"Год выпуска ТС: {restricted.VechicleYear}{Environment.NewLine}" +
                   $"Дата наложения ограничения: {restricted.RestrictedDate}{Environment.NewLine}" +
                   $"Регион инициатора ограничения: {restricted.RegionName}{Environment.NewLine}" +
                   $"Кем наложено ограничение: {restricted.InitiatorType}{Environment.NewLine}" +
                   $"Вид ограничения: {restricted.RestrictedType}{Environment.NewLine}" +
                   $"Основание ограничения: {restricted.RestrictedFoundations}{Environment.NewLine}" +
                   $"Телефон инициатора: {restricted.InitiatorPhone ?? "не указан"}";
        }


    }
}
