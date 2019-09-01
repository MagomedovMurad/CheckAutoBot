using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class VechiclePassportDataSource: IDataSource
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
            var auto = inputData as Auto;
            var cache = _queryExecutor.GetRequestObjectCacheItem(auto.Id, DataType.VechiclePassportData);

            var delta = (DateTime.Now - cache.DateTime).TotalHours;
            if (delta > 24)
                return null;

            var data = JsonConvert.DeserializeObject<VechiclePassportData>(cache.Data);
            return new DataSourceResult(data);
        }
    }
}
