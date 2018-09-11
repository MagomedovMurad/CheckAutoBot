using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public interface IRepositoryFactory
    {
        IBotRepository CreateBotRepository();
    }
}
