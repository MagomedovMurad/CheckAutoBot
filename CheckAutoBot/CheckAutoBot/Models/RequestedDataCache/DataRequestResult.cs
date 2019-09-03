using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Models.RequestedCaptchaCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Models.RequestedDataCache
{
    public class DataRequestResult
    {
        public int Id { get; set; }

        public DataSourceResult DataSourceResult { get; set; }

        public DataType? DataType { get; set; }

        public string DataSourceName { get; set; }

        public bool IsSuccessfull { get; set; }
    }
}
