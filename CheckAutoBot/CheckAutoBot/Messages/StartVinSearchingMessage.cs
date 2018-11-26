using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class StartVinSearchingMessage
    {
        public StartVinSearchingMessage(int requestObjectId)
        {
            RequestObjectId = requestObjectId;
        }
        public int RequestObjectId { get; set; }

    }
}
