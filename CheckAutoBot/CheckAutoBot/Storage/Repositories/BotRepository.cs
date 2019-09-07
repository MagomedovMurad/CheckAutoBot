using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public class BotRepository : IBotRepository
    {
        private readonly BotDbContext _dbContext;

        public BotRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public RequestObject GetLastUserRequestObject(int userId)
        {
            return _dbContext.RequestObjects
                                   .Where(x => x.UserId == userId)
                                   .OrderByDescending(x => x.Date)
                                   .FirstOrDefault();
        }

        public RequestObject GetUserRequestObject(int id)
        {
            return _dbContext.RequestObjects
                                   .FirstOrDefault(x => x.Id == id);
        }

        public Request GetUserRequest(int requestId)
        {
            return _dbContext.Requests
                                   .Include(x => x.RequestObject)
                                   .FirstOrDefault(x => x.Id == requestId);
        }

        public void AddRequestObject(RequestObject requestObject)
        {
            _dbContext.Add(requestObject);
            _dbContext.SaveChanges();
        }

        public int AddUserRequest(Request request)
        {
            var addedRequest = _dbContext.Add(request);
            _dbContext.SaveChanges();

            return addedRequest.Entity.Id;
        }

        public IEnumerable<RequestType> GetExecutedRequestTypes(int requestObjectId)
        {
            return  _dbContext.Requests
                                   .Where(x => x.RequestObjectId == requestObjectId && x.IsCompleted == true)
                                   .Select(x => x.Type).ToList();
        }

        public void UpdateVinCode(int requestObjectId, string vin)
        {
            var auto = _dbContext.RequestObjects.Where(x => x.Id == requestObjectId).OfType<Auto>().First();
            auto.Vin = vin;
            _dbContext.SaveChanges();
        }

        public void AddRequestObjectCacheItem(RequestObjectCache item)
        {
            _dbContext.Add(item);
            _dbContext.SaveChanges();
        }
        public RequestObjectCache GetRequestObjectCacheItem(int requestObjectId, DataType dataType)
        {
            return _dbContext.RequestObjectCache.SingleOrDefault(x => x.RequestObjectId == requestObjectId && x.DataType == dataType);
        }

        public void ChangeRequestStatus(int requestId, bool? state)
        {
            var request = _dbContext.Requests.FirstOrDefault(x => x.Id == requestId);
            request.IsCompleted = state;
            _dbContext.SaveChanges();
        }

        public bool ExistRequestsInProcess(int requestObjectId)
        {
            return _dbContext.Requests
                       .Any(x => x.RequestObjectId == requestObjectId && x.IsCompleted == null);
        }

        public bool MarkAsPaid(int requestObjectId)
        {
            var requestObject = _dbContext.RequestObjects.SingleOrDefault( x=> x.Id == requestObjectId);
            if (requestObject == null)
                return false;

            requestObject.IsPaid = true;
            _dbContext.SaveChanges();
            return true;
        }
    }
}
