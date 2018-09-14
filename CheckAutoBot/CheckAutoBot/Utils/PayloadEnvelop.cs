using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class PayloadEnvelop
    {
        public string DotNetType { get; set; }

        public object Payload { get; set; }
    }
}
