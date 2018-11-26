using CheckAutoBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class CacheItem
    {
        public int Id { get; set; }

        public ActionType ActionType { get; set; }

        public int AttemptsCount { get; set; }

        public bool? DcGetFailed { get; set; }
    }
}
