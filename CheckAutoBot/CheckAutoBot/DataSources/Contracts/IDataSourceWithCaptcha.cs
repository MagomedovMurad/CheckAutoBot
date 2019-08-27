using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources.Contracts
{
    public interface IDataSourceWithCaptcha : IDataSource
    {
        IEnumerable<CaptchaRequestData> RequestCaptcha();
    }
}
