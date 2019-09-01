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
    public class WantedDataSource : IDataSourceWithCaptcha
    {
        private RucaptchaManager _rucaptchaManager;
        private GibddManager _gibddManager;

        public WantedDataSource(RucaptchaManager rucaptchaManager, GibddManager gibddManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _gibddManager = gibddManager;
        }

        public DataType DataType => DataType.Wanted;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var auto = inputData as Auto;
            var captchaRequestData = captchaRequestItems.Single();
            var wantedResult = _gibddManager.GetWanted(auto.Vin, captchaRequestData.Value, captchaRequestData.SessionId);

            if(wantedResult is null)
                return new DataSourceResult() { Data = new WantedData() };

            var wantedAccidents = wantedResult.Wanteds.Select(x => new WantedAccident()
            {
                Date = x.Date,
                Order = x.Order,
                RecordType = x.RecordType,
                RegionIniciator = x.RegionIniciator,
                VechicleModel = x.VechicleModel,
                VechicleYear = x.VechicleYear
            });

            return new DataSourceResult(new WantedData() { Accidents = wantedAccidents });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.RequestPingbackUrl, 3, "check_auto_wanted");
            return new[] { new CaptchaRequestData(captchaRequest.Id, null, string.Empty) };
        }
    }
}
