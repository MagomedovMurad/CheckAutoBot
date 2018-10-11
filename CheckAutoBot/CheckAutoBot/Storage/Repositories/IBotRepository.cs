using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public interface IBotRepository: IDisposable
    {
        Task<RequestObject> GetLastUserRequestObject(int userId);

        Task AddRequestObject(RequestObject requestObject);

        Task<int> AddUserRequest(Request request);

        Task<Request> GetUserRequest(int requestId);

        Task<IEnumerable<RequestType>> GetRequestTypes(int requestObjectId);

        Task UpdateVinCode(int requestObjectId, string vin);

    }
}
