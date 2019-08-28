using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Enums;
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

        public DataType DataType => DataType.DiagnosticCards;

        public int MaxRepeatCount => 2;

        public int Order => 1;

        public object GetData(object data)
        {
           
        }

        public object GetData(object data, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            throw new NotImplementedException();
        }
    }
}
