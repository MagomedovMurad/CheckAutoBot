using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class VechiclePassportDataSource: IDataSourceWithCaptcha
    {
        private DbQueryExecutor _queryExecutor;

        public VechiclePassportDataSource(DbQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        public DataType DataType => DataType.VechiclePassportData;

        public int MaxRepeatCount => 2;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var data = inputData as string;

            var cache = _queryExecutor.GetAutoObjectByVin();
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            throw new NotImplementedException();
        }
    }
}
