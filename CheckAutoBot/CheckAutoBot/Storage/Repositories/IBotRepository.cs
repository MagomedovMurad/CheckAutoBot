using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public interface IBotRepository: IDisposable
    {
        Task<RequestObject> GetLastUserRequestObject(int userId);
    }
}
