using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class MessagesAllowedEventMessage
    {
        public MessagesAllowedEventMessage(int userId)
        {
            UserId = userId;
        }
        public int UserId { get; set; }
    }
}
