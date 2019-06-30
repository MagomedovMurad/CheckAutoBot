using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace CheckAutoBot.Utils
{
    public interface ICustomLogger
    {
        void WriteToLog(LogLevel logLevel, string message, bool sendToUser = false);
    }
}
