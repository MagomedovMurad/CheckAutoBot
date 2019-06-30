using CheckAutoBot.Enums;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Contracts
{
    public interface IVinFinderHandler
    {
        ActionType SupportedActionType { get; }

        PreGetResult PreGet();

        string Get(string licensePlate, IEnumerable<ActionCacheItem> cacheItems);
    }
}
