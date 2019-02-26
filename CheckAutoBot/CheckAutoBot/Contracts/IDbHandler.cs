using CheckAutoBot.Enums;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Contracts
{
    public interface IDbHandler
    {
        ActionType SupportedActionType { get; }

        Task<Dictionary<string, byte[]>> Get(RequestObject auto);
    }
}
