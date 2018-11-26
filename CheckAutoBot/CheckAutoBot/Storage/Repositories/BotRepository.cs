using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public class BotRepository: IBotRepository
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

        public async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            return await _dbContext.RequestObjects
                                   .Where(x => x.UserId == userId)
                                   .OrderByDescending(x => x.Date)
                                   .FirstOrDefaultAsync();
        }

        public async Task<RequestObject> GetUserRequestObject(int id)
        {
            return await _dbContext.RequestObjects
                                   .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Request> GetUserRequest(int requestId)
        {
            return await _dbContext.Requests
                                   .Include(x => x.RequestObject)
                                   .FirstOrDefaultAsync(x => x.Id == requestId);
        }

        public async Task AddRequestObject(RequestObject requestObject)
        {
            await _dbContext.AddAsync(requestObject);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> AddUserRequest(Request request)
        {
            var addedRequest = await _dbContext.AddAsync(request);
            await _dbContext.SaveChangesAsync();

            return addedRequest.Entity.Id;
        }

        public async Task<IEnumerable<RequestType>> GetExecutedRequestTypes(int requestObjectId)
        {
            return await _dbContext.Requests
                                   .Where(x => x.RequestObjectId == requestObjectId && x.IsCompleted)
                                   .Select(x => x.Type).ToListAsync();
        }

        public async Task UpdateVinCode(int requestObjectId, string vin)
        {
            var auto = _dbContext.RequestObjects.Where(x => x.Id == requestObjectId).OfType<Auto>().First();
            auto.Vin = vin;
            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkRequestCompleted(int requestId)
        {
            var request = _dbContext.Requests.FirstOrDefault(x => x.Id == requestId);
            request.IsCompleted = true;
            await _dbContext.SaveChangesAsync();
        }

    }
}
