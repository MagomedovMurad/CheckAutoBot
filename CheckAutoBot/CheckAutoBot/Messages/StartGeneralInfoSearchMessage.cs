using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class StartGeneralInfoSearchMessage
    {
        public string Vin { get; set; }

        public int RequestObjectId { get; set; }
    }
}
