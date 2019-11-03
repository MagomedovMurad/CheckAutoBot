using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
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

        public DataType DataType => DataType.CurrentDiagnosticCard;

        public int MaxRepeatCount => 2;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            throw new NotImplementedException();
        }
    }
}
