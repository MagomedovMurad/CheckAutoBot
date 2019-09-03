using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Models.RequestedCaptchaCache;
using CheckAutoBot.Models.RequestedDataCache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Models.RepeatedRequestCache
{
    public class DataRequest
    {
        public int Id { get; set; }

        public IDataSource DataSource { get; set; }

        public int RepeatCount { get; set; }

        public object InputData { get; set; }

        public Func<DataRequestResult, Task> CallBack { get; set; }
    }
}
