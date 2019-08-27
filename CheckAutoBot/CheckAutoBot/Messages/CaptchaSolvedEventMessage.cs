using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Messages
{
    public class CaptchaSolvedEventMessage
    {
        public string CaptchaId { get; set; }

        public string Answer { get; set; }
    }
}
