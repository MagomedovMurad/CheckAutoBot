using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.PledgeModels;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class PledgeHandler : IHandler
    {
        private FnpManager _fnpManager;
        private RucaptchaManager _rucaptchaManager;

        public PledgeHandler(FnpManager fnpManager, RucaptchaManager rucaptchaManager)
        {
            _fnpManager = fnpManager;
            _rucaptchaManager = rucaptchaManager;
        }

        public ActionType SupportedActionType => ActionType.Pledge;

        public Dictionary<string, byte[]> Get(RequestObject requestObject, CaptchaCacheItem cacheItem)
        {
            var auto = requestObject as Auto;

            //var pledgeCacheItem = cacheItems.First(x => x.ActionType == ActionType.Pledge);
            var pledgeResponse = _fnpManager.GetPledges(auto.Vin, cacheItem.CaptchaWord, cacheItem.SessionId);

            return GenerateResponse(pledgeResponse);
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _fnpManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.RequestPingbackUrl);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }

        private Dictionary<string, byte[]> GenerateResponse(PledgeResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "В базе ФНП не найдены сведения о нахождении транспортного средства в залоге";
                messages.Add(text, null);
            }
            else
            {
                foreach (var pledge in result.Pledges)
                {
                    var text = PledgeToText(pledge);
                    messages.Add(text, null);
                }
            }

            return messages;
        }

        private string PledgeToText(PledgeListItem pledge)
        {
            var text = $"Уведомление о возникновении залога №{pledge.ReferenceNumber} {Environment.NewLine}";
            text += $"Дата регистрации: {pledge.RegisterDate}{Environment.NewLine}";
            text += $"Залогодатель: {string.Join(Environment.NewLine, pledge.Pledgors.Select(x => PledgorToText(x)))}";
            text += $"Залогодержатель: {string.Join(Environment.NewLine, pledge.Pledgees.Select(x => PledgeeToText(x)))}";
            text += Environment.NewLine;

            return text;
        }

        private string PledgorToText(Pledgor pledgor)
        {
            var text = pledgor.Type == SubjectType.Person ? "Физическое лицо" : "Юридическое лицо";
            return text += Environment.NewLine;
        }

        private string PledgeeToText(Pledgee pledgee)
        {
            var text = pledgee.Type == SubjectType.Organization ? "Юридическое лицо" : "Физическое лицо";
            return text += Environment.NewLine + pledgee.Name + Environment.NewLine;
        }
    }
}
