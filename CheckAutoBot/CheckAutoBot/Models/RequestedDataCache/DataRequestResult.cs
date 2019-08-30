using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;
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

        public bool IsSuccessfull { get; set; }
    }
}
