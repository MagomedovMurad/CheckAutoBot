﻿using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Models.RepeatedRequestCache;
using CheckAutoBot.Models.RequestedDataCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckAutoBot.Controllers
{
    public class RequestedDataCacheController
    {
        private List<DataRequest> _dataRequest;

        public RequestedDataCacheController()
        {
            _dataRequest = new List<DataRequest>();
        }

        public void Add(int id, IDataSource dataSource, object inputData, Func<DataRequestResult, Task> callBack)
        {
            var request = new DataRequest()
            {
                Id = id,
                InputData = inputData,
                DataSource = dataSource,
                CallBack = callBack,
                RepeatCount = 1
            };

            _dataRequest.Add(request);
        }

        public DataRequest Get(int id)
        {
            return _dataRequest.Single(x => x.Id.Equals(id));
        }

        public void UpRepeatCount(int id)
        {
            var dataRequest = _dataRequest.Single(x => x.Id.Equals(id));
            dataRequest.RepeatCount++;
        }
        public void UpdateDataSource(int id, IDataSource dataSource)
        {
            var dataRequest = _dataRequest.Single(x => x.Id.Equals(id));
            dataRequest.RepeatCount = 1;
            dataRequest.DataSource = dataSource;
        }
    }
}