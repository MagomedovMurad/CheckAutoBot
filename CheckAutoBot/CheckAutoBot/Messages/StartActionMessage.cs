using CheckAutoBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class StartActionMessage
    {
        public int RequestId { get; set; }

        public ActionType ActionType { get; set; }

    }
}
