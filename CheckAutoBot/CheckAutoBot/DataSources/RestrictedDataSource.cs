using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
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
    public class RestrictedDataSource : IDataSourceWithCaptcha
    {
        private RucaptchaManager _rucaptchaManager;
        private GibddManager _gibddManager;

        public RestrictedDataSource(RucaptchaManager rucaptchaMana, GibddManager gibddManager)
        {
            _rucaptchaManager = rucaptchaMana;
            _gibddManager = gibddManager;
        }

        public string Name => "GIBDD_RESTRICTEDS";
        public DataType DataType => DataType.Restricted;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var auto = inputData as Auto;
            var captchaRequestData = captchaRequestItems.Single();
            var restrictedResult = _gibddManager.GetRestrictions(auto.Vin, captchaRequestData.Value, captchaRequestData.SessionId);

            if (restrictedResult is null)
                return new DataSourceResult(new RestrictedData());

            var restrictedAccidents = restrictedResult.Restricteds.Select(x => new RestrictedAccident()
            {
                DateAdd = x.DateAdd,
                FrameNumber = x.TsBody,
                InitiatorPhone = x.InitiatorPhone,
                InitiatorType = x.InitiatorType,
                RegionName = x.RegionName,
                RestrictedDate = x.RestrictedDate,
                RestrictedFoundations = x.RestrictedFoundations,
                RestrictedType = x.RestrictedType,
                VechicleModel = x.TsModel,
                VechicleYear = x.VechicleYear,
                Vin = x.Vin
            });

            return new DataSourceResult(new RestrictedData() { Accidents = restrictedAccidents });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.pageUrl, Rucaptcha.RequestPingbackUrl, 3, "check_auto_restricted");
            return new[] { new CaptchaRequestData(captchaRequest.Id, null, "") };
        }
    }
}
