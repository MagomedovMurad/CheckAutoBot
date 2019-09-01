using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.FnpModels;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class PledgeDataSource : IDataSourceWithCaptcha
    {
        private FnpManager _fnpManager;
        private RucaptchaManager _rucaptchaManager;

        public PledgeDataSource()
        {

        }

        
        public DataType DataType => DataType.Pledge;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var auto = inputData as Auto;
            var captchaRequestData = captchaRequestItems.Single();
            var pledgeResponse = _fnpManager.GetPledges(auto.Vin, captchaRequestData.Value, captchaRequestData.SessionId);

            if (pledgeResponse is null)
                return new DataSourceResult(new PledgeData());

            var pledgeAccidents = pledgeResponse.Data.Select(x => new PledgeAccident()
            {
                Id = x.Id,
                Pledgees = x.Pledgees.Select(p =>
                {
                    var pledgee = PledgeeToText(p);

                    return new Infrastructure.Models.DataSource.Pledgee() { Type = pledgee.Key, Name = pledgee.Value };
                }).ToList(),
                Pledgors = x.Pledgors.Select(p =>
                {
                    var pledgor = PledgorToText(p);
                    return new Infrastructure.Models.DataSource.Pledgor() { Type = pledgor.Key, Name = pledgor.Value };
                }).ToList(),
                RegistrationDate = x.RegistrationDate
            });

            return new DataSourceResult(new PledgeData() { Accidents = pledgeAccidents });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaResult = _fnpManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.RequestPingbackUrl);
            return new[] { new CaptchaRequestData(captchaRequest.Id, captchaResult.SessionId, string.Empty) };
        }

        private KeyValuePair<SubjectType, string> PledgorToText(FnpModels.Pledgor pledgor)
        {
            if (pledgor.Organization != null)
                return new KeyValuePair<SubjectType, string>(SubjectType.Organization, pledgor.Organization);

            if (pledgor.PrivatePerson != null)
                return new KeyValuePair<SubjectType, string>(SubjectType.PrivatePerson, PrivatePersonToString(pledgor.PrivatePerson));

            if (pledgor.SoleProprietorship != null)
                return new KeyValuePair<SubjectType, string>(SubjectType.SoleProprietor, SoleProprietorshipToString(pledgor.SoleProprietorship));

            throw new InvalidOperationException("Неизвестный тип субъекта");
        }

        private string PrivatePersonToString(PrivatePerson person)
        {
            return $"{person.Name}, {person.Birthday.ToString("dd.MM.yyyy")}";
        }

        private string SoleProprietorshipToString(SoleProprietorship soleProprietorship)
        {
            return $"{soleProprietorship.Name}, {soleProprietorship.Birthday}, ОГРНИП:{soleProprietorship.Ogrnip}";
        }

        private KeyValuePair<SubjectType, string> PledgeeToText(FnpModels.Pledgee pledgee)
        {
            if (pledgee.Organization != null)
                return new KeyValuePair<SubjectType, string>(SubjectType.Organization, pledgee.Organization.Name);

            if (pledgee.PrivatePerson != null)
                return new KeyValuePair<SubjectType, string>(SubjectType.PrivatePerson, PrivatePersonToString(pledgee.PrivatePerson));

            throw new InvalidOperationException("Неизвестный тип субъекта");
        }
    }
}
