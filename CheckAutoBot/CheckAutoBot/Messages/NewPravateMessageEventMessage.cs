using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Messages
{
    public class NewPravateMessageEventMessage
    {
        public NewPravateMessageEventMessage(PrivateMessage message)
        {
            Message = message;
        }
        public PrivateMessage Message { get; set; }
    }
}
