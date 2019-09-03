using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
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
    public class OwnershipPeriodsDataSource : IDataSource
    {
        DbQueryExecutor _queryExecutor;

        public OwnershipPeriodsDataSource(DbQueryExecutor dbQueryExecutor)
        {
            _queryExecutor = dbQueryExecutor;
        }

        public string Name => "GIBDD_OWNERS";

        public DataType DataType => DataType.OwnershipPeriods;

        public int MaxRepeatCount => 1;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var auto = inputData as Auto;
            var cache = _queryExecutor.GetRequestObjectCacheItem(auto.Id, DataType.OwnershipPeriods);

            var delta = (DateTime.Now - cache.DateTime).TotalHours;
            if (delta > 24)
                return null;

            var data = JsonConvert.DeserializeObject<OwnershipPeriodPata>(cache.Data);
            return new DataSourceResult(data);
        }
    }
}
