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

    }
}
