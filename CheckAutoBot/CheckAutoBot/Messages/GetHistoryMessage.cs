using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class GetHistoryMessage
    {
        public string Number { get; set; }

        public string Vin { get; set; }
    }
}
