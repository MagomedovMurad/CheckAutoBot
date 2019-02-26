using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Contracts
{
    public interface IHttpHandler
    {
        ActionType SupportedActionType { get; }

        PreGetResult PreGet();

        Dictionary<string, byte[]> Get(RequestObject auto, string captchaWord, string sessionId);
    }
}
