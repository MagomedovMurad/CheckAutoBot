﻿using Microsoft.EntityFrameworkCore;
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

    }
}
