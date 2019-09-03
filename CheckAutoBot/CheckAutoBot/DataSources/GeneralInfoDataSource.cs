using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwnershipPeriod = CheckAutoBot.Infrastructure.Models.DataSource.OwnershipPeriod;

namespace CheckAutoBot.DataSources
{
    public class GeneralInfoDataSource : IDataSourceWithCaptcha
    {
        protected GibddManager _gibddManager;
        protected RucaptchaManager _rucaptchaManager;

        public string Name => "GIBDD_GENERAL_INFO";

        public DataType DataType => DataType.GeneralInfo;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object data, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var vin = data as string;
            var captcha = captchaRequestItems.First();

            var result = _gibddManager.GetHistory(vin, captcha.Value, captcha.SessionId);
            return ConvertData(result);
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.VinPingbackUrl, 3, "check_auto_history");
            return new[] { new CaptchaRequestData(captchaRequest.Id, null, null) };
        }

        private DataSourceResult ConvertData(HistoryResult history)
        {
            var generalInfo = new GeneralInfo()
            {
                Model = history.Vehicle.Model,
                Year = history.Vehicle.Year
            };

            var vechiclePassportData = new VechiclePassportData()
            {
                Category = history?.Vehicle?.Category,
                Color = history?.Vehicle?.Color,
                EngineNumber = history?.Vehicle?.EngineNumber,
                CompanyName = history?.VehiclePassport?.CompanyName,
                EngineVolume = history?.Vehicle?.EngineVolume,
                FrameNumber = history?.Vehicle?.BodyNumber,
                Model = history?.Vehicle?.Model,
                PowerHp = history?.Vehicle?.PowerHp,
                PowerKwt = history?.Vehicle?.PowerKwt,
                PTSNumber = history?.VehiclePassport?.Number,
                Type = history?.Vehicle?.TypeName,
                Vin = history?.Vehicle?.Vin,
                Year = history?.Vehicle?.Year
            };

            var periods = history?.OwnershipPeriodsEnvelop?.OwnershipPeriods?.Select(x =>
            {
                return new OwnershipPeriod()
                {
                    OwnerType = x.OwnerType == Enums.OwnerType.Legal ? Infrastructure.Enums.OwnerType.Legal : Infrastructure.Enums.OwnerType.Natural,
                    DateFrom = x.From,
                    DateTo = x.To,
                    LastOperation = x.LastOperation,
                    LastOperationCode = x.LastOperationCode
                };
            }).ToList();

            var ownershipPeriodData = new OwnershipPeriodPata()
            {
                OwnershipPeriods = periods
            };

            return new DataSourceResult()
            {
                Data = generalInfo,
                RelatedData = new[] { new RelatedData(vechiclePassportData, DataType.VechiclePassportData),
                                      new RelatedData(ownershipPeriodData, DataType.OwnershipPeriods)  }
            };
        }
    }
}
