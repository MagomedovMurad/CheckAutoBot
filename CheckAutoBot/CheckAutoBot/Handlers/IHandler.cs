﻿using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Handlers
{
    public interface IHandler
    {
        ActionType SupportedActionType { get; }

        PreGetResult PreGet();

        Dictionary<string, byte[]> Get(RequestObject auto, IEnumerable<CaptchaCacheItem> cacheItems);
    }
}
