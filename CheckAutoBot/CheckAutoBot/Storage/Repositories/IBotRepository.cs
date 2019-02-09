using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public interface IBotRepository: IDisposable
    {
        Task<RequestObject> GetLastUserRequestObject(int userId);

        Task<RequestObject> GetUserRequestObject(int id);

        Task AddRequestObject(RequestObject requestObject);

        Task<int> AddUserRequest(Request request);

        Task<Request> GetUserRequest(int requestId);

        Task<IEnumerable<RequestType>> GetExecutedRequestTypes(int requestObjectId);

        Task UpdateVinCode(int requestObjectId, string vin);

        Task ChangeRequestStatus(int requestId, bool? state);

        Task<bool> ExistRequestsInProcess(int requestObjectId);

        Task<bool> MarkAsPaid(int requestObjectId);
    }
}
