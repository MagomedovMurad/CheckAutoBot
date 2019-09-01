using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class EaistoDataSource : IDataSourceWithCaptcha
    {
        private EaistoManager _eaistoManager;
        private RucaptchaManager _rucaptchaManager;

        public EaistoDataSource(EaistoManager eaistoManager, RucaptchaManager rucaptchaManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _eaistoManager = eaistoManager;
        }

        public DataType DataType => DataType.DiagnosticCards;

        public int MaxRepeatCount => 2;

        public int Order => 2;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;
            var captcha = captchaRequestItems.First();

            var diagnosticCards = _eaistoManager.GetLastDiagnosticCards(captcha.Value, null, captcha.SessionId, licensePlate: licensePlate);
            if (diagnosticCards == null)
                return null;

            var dcs = diagnosticCards.Select(x => new DiagnosticCard()
            {
                Brand = x.Brand,
                DateFrom = x.DateFrom,
                DateTo = x.DateTo,
                EaistoNumber = x.EaistoNumber,
                FrameNumber = x.EaistoNumber,
                LicensePlate = x.LicensePlate,
                Model = x.Model,
                Operator = x.Model,
                Vin = x.Vin
            });

            return new DataSourceResult(new DiagnosticCardsData() { DiagnosticCards = dcs});
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.LpPingbackUrl);

            return new[] { new CaptchaRequestData(captchaRequest.Id, captchaResult.SessionId, "") };
        }

        //public object GetData(object data, IEnumerable<CaptchaRequestData> captchaRequestItems)
        //{
        //    var licensePlate = data as string;
        //    var captcha = captchaRequestItems.First();

        //    var lastDiagnosticCard = _eaistoManager.GetLastDiagnosticCard(captcha.Value, null, captcha.SessionId, licensePlate: licensePlate);
        //    if (lastDiagnosticCard == null)
        //        return null;

        //    return lastDiagnosticCard;
        //}

    }
}
