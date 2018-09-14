using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class UserInputDataMessage
    {
        public int UserId { get; set; }

        public int MessageId { get; set; }

        public string Data { get; set; }

        public InputDataType Type { get; set; }

        public DateTime Date { get; set; }
    }
}
