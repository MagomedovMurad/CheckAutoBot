using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
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

        public DataType DataType => DataType.OwnershipPeriods;

        public int MaxRepeatCount => 1;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            int? id = inputData as int?;
            var cache = _queryExecutor.GetRequestObjectCacheItem(id.Value, DataType.OwnershipPeriods);
        }
    }
}
