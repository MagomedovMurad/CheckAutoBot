using CheckAutoBot.Vk.Api.MessagesModels;
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

        public Keyboard Keyboard { get; set; }
    }
}
