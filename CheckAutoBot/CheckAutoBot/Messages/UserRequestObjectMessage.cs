using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class UserRequestObjectMessage
    {
        public int UserId { get; set; }

        public int MessageId { get; set; }

        public string Data { get; set; }

        public RequestObjectType Type { get; set; }

        public DateTime Date { get; set; }
    }
}
