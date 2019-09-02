using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class EaistoMeDataSource : IDataSourceWithCaptcha
    {
        public DataType DataType => throw new NotImplementedException();

        public int MaxRepeatCount => throw new NotImplementedException();

        public int Order => throw new NotImplementedException();

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            throw new NotImplementedException();
        }
    }
}
