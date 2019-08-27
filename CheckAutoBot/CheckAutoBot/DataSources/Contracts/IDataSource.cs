using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources.Contracts
{
    public interface IDataSource
    {
        DataType DataType { get; }

        int MaxRepeatCount { get; }

        int Order { get; }

        DataSourceResult GetData(object data, IEnumerable<CaptchaRequestData> captchaRequestItems);
    }
}
