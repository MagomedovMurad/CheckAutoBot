using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class SendToUserMessage
    {
        public int UserId { get; set; }

        public string Text { get; set; }

        public byte[] Photo { get; set; }
    }
}
