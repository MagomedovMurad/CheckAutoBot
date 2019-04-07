using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.FnpModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class PledgeHandler : IHttpHandler
    {
        private FnpManager _fnpManager;
        private RucaptchaManager _rucaptchaManager;

        public PledgeHandler(FnpManager fnpManager, RucaptchaManager rucaptchaManager)
        {
            _fnpManager = fnpManager;
            _rucaptchaManager = rucaptchaManager;
        }

        public ActionType SupportedActionType => ActionType.Pledge;

        public Dictionary<string, byte[]> Get(RequestObject requestObject, string captchaWord, string sessionId)
        {
            var auto = requestObject as Auto;

            //var pledgeCacheItem = cacheItems.First(x => x.ActionType == ActionType.Pledge);
            var pledgeResponse = _fnpManager.GetPledges(auto.Vin, captchaWord, sessionId);

            return GenerateResponse(pledgeResponse);
        }

        public PreGetResult PreGet()
        {
            var captchaResult = _fnpManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.RequestPingbackUrl);

            return new PreGetResult(captchaRequest.Id, captchaResult.SessionId);
        }

        private Dictionary<string, byte[]> GenerateResponse(PledgeResponse response)
        {
            var messages = new Dictionary<string, byte[]>();
            var pledges = response.Data.Where(x => x.History.FirstOrDefault(h => h.Type == HistoryItemType.Exclusion) == null);
            if (pledges.Any())
            {
                for (int i = 0; i < pledges.Count(); i++)
                {
                    var text = PledgeToText(pledges.ElementAt(i), i + 1);
                    messages.Add(text, null);
                }
            }
            else
            {
                var text = "✅ В базе ФНП не найдены сведения о нахождении транспортного средства в залоге";
                messages.Add(text, null);
            }
            return messages;
        }

        private string PledgeToText(Pledge pledge, int number)
        {
            var text = $"📃 {number}. Уведомление о возникновении залога №{pledge.Id} {Environment.NewLine}";
            text += $"Дата регистрации: {pledge.RegistrationDate.ToString("dd.MM.yyyy H:mm:ss")}{Environment.NewLine}";
            text += $"Залогодатель: {string.Join(Environment.NewLine, pledge.Pledgors.Select(x => PledgorToText(x)))} {Environment.NewLine}";
            text += $"Залогодержатель: {string.Join(Environment.NewLine, pledge.Pledgees.Select(x => PledgeeToText(x)))}";
            text += Environment.NewLine;

            return text;
        }

        private string PledgorToText(Pledgor pledgor)
        {
            if (pledgor.Organization != null)
                return $"Юридическое лицо, {pledgor.Organization}";

            if (pledgor.PrivatePerson != null)
                return PrivatePersonToString(pledgor.PrivatePerson);

            if (pledgor.SoleProprietorship != null)
                return SoleProprietorshipToString(pledgor.SoleProprietorship);

            var text = pledgor.Organization ?? 
                    PrivatePersonToString(pledgor.PrivatePerson) ?? 
                    SoleProprietorshipToString(pledgor.SoleProprietorship);

            return text += Environment.NewLine;
        }

        private string PrivatePersonToString(PrivatePerson person)
        {
            return $"Физическое лицо, " +
                   $"{person.Name}, {person.Birthday.ToString("dd.MM.yyyy")}";
        }

        private string SoleProprietorshipToString(SoleProprietorship soleProprietorship)
        {
            return $"Индивидуальный предприниматель, " +
                   $"{soleProprietorship.Name}, {soleProprietorship.Birthday}, ОГРНИП:{soleProprietorship.Ogrnip}";
        }

        private string PledgeeToText(Pledgee pledgee)
        {
            string text = null;
            if (pledgee.Organization != null)
                text = OrganizationToString(pledgee.Organization);

            if (pledgee.PrivatePerson != null)
                text = PrivatePersonToString(pledgee.PrivatePerson);

            return text += Environment.NewLine;
        }

        private string OrganizationToString(Organization organization)
        {
            return $"Юридическое лицо, {organization.Name}";
        }
    }
}
