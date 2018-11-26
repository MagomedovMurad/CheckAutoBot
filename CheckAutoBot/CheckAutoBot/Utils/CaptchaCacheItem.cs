using CheckAutoBot.Enums;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class CaptchaCacheItem
    {
        public string CaptchaId { get; set; }

        public int Id { get; set; }

        public ActionType ActionType { get; set; }

        public string CaptchaWord { get; set; }

        public string SessionId { get; set; }

        public DateTime Date { get; set; }

    }
}
