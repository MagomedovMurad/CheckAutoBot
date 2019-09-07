using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using System.Collections.Generic;
using System.Linq;

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

        public string Name => "EAISTO_VECHICLE_IDENTIFIERS";

        public DataType DataType => DataType.VechicleIdentifiersEAISTO;

        public int MaxRepeatCount => 2;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;
            var captcha = captchaRequestItems.First();

            var diagnosticCards = _eaistoManager.GetDiagnosticCards(captcha.Value, null, captcha.SessionId, licensePlate: licensePlate);
            if (diagnosticCards == null)
                return new DataSourceResult(null);
            //throw new DataNotFoundException();

            var dcs = diagnosticCards.Select(x => new DiagnosticCard()
            {
                Brand = x.Brand,
                DateFrom = x.DateFrom,
                DateTo = x.DateTo,
                EaistoNumber = x.EaistoNumber,
                FrameNumber = x.FrameNumber,
                LicensePlate = x.LicensePlate,
                Model = x.Model,
                Operator = x.Model,
                Vin = x.Vin
            });

            var lastDc = dcs.FirstOrDefault();

            var vechicleIdentifiers = new VechicleIdentifiersData() { FrameNumber = lastDc.FrameNumber, Vin = lastDc.Vin, LicensePlate = lastDc.LicensePlate };
            var relatedData = new RelatedData(new DiagnosticCardsData() { DiagnosticCards = dcs }, DataType.DiagnosticCards);
            return new DataSourceResult(vechicleIdentifiers, new[] { relatedData });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.LpPingbackUrl);

            return new[] { new CaptchaRequestData(captchaRequest.Id, captchaResult.SessionId, null) };
        }
    }
}
