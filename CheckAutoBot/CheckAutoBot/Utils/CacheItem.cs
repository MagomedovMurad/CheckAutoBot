using CheckAutoBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class CacheItem
    {
        public int RequestId { get; set; }

        public ActionType CurrentActionType { get; set; }

        public int AttemptsCount { get; set; }
    }
}
