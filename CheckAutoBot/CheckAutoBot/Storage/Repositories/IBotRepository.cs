using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public interface IBotRepository: IDisposable
    {
        RequestObject GetLastUserRequestObject(int userId);

        RequestObject GetUserRequestObject(int id);

        void AddRequestObject(RequestObject requestObject);

        int AddUserRequest(Request request);

        Request GetUserRequest(int requestId);

        IEnumerable<RequestType> GetExecutedRequestTypes(int requestObjectId);

        void UpdateVinCode(int requestObjectId, string vin);

        void ChangeRequestStatus(int requestId, bool? state);

        bool ExistRequestsInProcess(int requestObjectId);

        bool MarkAsPaid(int requestObjectId);

        void AddRequestObjectCacheItem(RequestObjectCache item);

        RequestObjectCache GetRequestObjectCacheItem(int requestObjectId, DataType dataType);
    }
}
