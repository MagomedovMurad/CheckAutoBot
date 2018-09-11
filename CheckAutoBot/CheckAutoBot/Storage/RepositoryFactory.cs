using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class RepositoryFactory: IRepositoryFactory
    {
        public IBotRepository CreateBotRepository() => new BotRepository(new BotDbContext());
    }
}
