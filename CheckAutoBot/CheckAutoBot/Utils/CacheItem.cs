using CheckAutoBot.Enums;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class CacheItem
    {
        public long CaptchaId { get; set; }

        public int RequestId { get; set; }

        public ActionType CurrentActionType { get; set; }

        public ActionType TargetActionType { get; set; }

        public string CaptchaWord { get; set; }

        public string SessionId { get; set; }

        public int AttemptsCount { get; set; }

        public DateTime Date { get; set; }
    }
}
