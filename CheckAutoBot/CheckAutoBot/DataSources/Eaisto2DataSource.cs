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
    public class Eaisto2DataSource : IDataSource
    {
        private EaistoManager _eaistoManager;

        public Eaisto2DataSource(EaistoManager eaistoManager)
        {
            _eaistoManager = eaistoManager;
        }
        public string Name => "EAISTO_DIAGNOSTIC_CARDS2";

        public DataType DataType => DataType.VechicleIdentifiersEAISTO;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;
            var dc = _eaistoManager.GetLastDiagnosticCard(licensePlate);

            var vechicleIdentifiers = new VechicleIdentifiersData()
            {
                FrameNumber = dc.FrameNumber,
                Vin = dc.Vin,
                LicensePlate = dc.LicensePlate
            };
            return new DataSourceResult(vechicleIdentifiers);
        }
    }
}
