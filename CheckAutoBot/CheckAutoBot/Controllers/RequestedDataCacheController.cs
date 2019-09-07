using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Models.RepeatedRequestCache;
using CheckAutoBot.Models.RequestedDataCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckAutoBot.Controllers
{
    public interface IRequestedDataCacheController
    {
        void Add(int id, IDataSource dataSource, object inputData, Func<DataRequestResult, Task> callBack);
        DataRequest Get(int id);
        void UpRepeatCount(int id);
        void UpdateDataSource(int id, IDataSource dataSource);
        void Remove(int id);
    }

    public class RequestedDataCacheController: IRequestedDataCacheController
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
                RepeatCount = 1,
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

        public void Remove(int id)
        {
            _dataRequest.RemoveAll(x => x.Id == id);
        }
    }
}
