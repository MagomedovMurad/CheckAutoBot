using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Utils
{
    public interface ILicensePlateControllerCache
    {
        void Add(int requestObjectId, DataType requestedDataType, string licensePlate);

        void Update(int requestObjectId, DataType requestedDataType, bool dcSourcesIsAvailable);

        LPRequestedData Get(int requestObjectId);

        void Remove(int requestObjectId);
    }
    public class LicensePlateControllerCache : ILicensePlateControllerCache
    {
        private List<LPRequestedData> _items = new List<LPRequestedData>();

        public void Add(int requestObjectId, DataType requestedDataType, string licensePlate)
        {
            var item = new LPRequestedData()
            {
                RequestObjectId = requestObjectId,
                RequestedDataType = requestedDataType,
                LicensePlate = licensePlate
            };
            _items.Add(item);
        }

        public void Update(int requestObjectId, DataType requestedDataType, bool dcSourcesIsAvailable)
        {
            var data = _items.Single(x => x.RequestObjectId == requestObjectId);
            data.RequestedDataType = requestedDataType;
            data.DCSourcesNotAvailable = true;
        }

        public LPRequestedData Get(int requestObjectId)
        {
            return _items.Single(x => x.RequestObjectId == requestObjectId);
        }

        public void Remove(int requestObjectId)
        {
            _items.RemoveAll(x => x.RequestObjectId == requestObjectId);
        }
    }

    public class LPRequestedData
    {
        public int RequestObjectId { get; set; }

        public DataType RequestedDataType { get; set; }

        public string LicensePlate { get; set; }

        public bool DCSourcesNotAvailable { get; set; }
    }
}
