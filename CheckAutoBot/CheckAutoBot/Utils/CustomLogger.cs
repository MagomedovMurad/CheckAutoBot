using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using CheckAutoBot.Vk.Api;

namespace CheckAutoBot.Utils
{
    public class CustomLogger: ICustomLogger
    {
        ILogger _logger;
        VkApiManager _vkApiManager;
        public CustomLogger(VkApiManager vkApiManager)
        {
            LogManager.LoadConfiguration("NLog.config");
            _logger = LogManager.GetCurrentClassLogger();
            _vkApiManager = vkApiManager;
        }

        public void WriteToLog(LogLevel logLevel, string message, bool sendToUser)
        {
            if (logLevel == LogLevel.Debug)
                _logger.Debug(message);
            else if (logLevel == LogLevel.Warn)
                _logger.Warn(message);
            else if (logLevel == LogLevel.Error)
                _logger.Error(message);
            else throw new Exception("Specified log level not supported");

            if (sendToUser)
                _vkApiManager.SendMessage(StaticResources.MyUserId, message, null, null);
        }

        
    }
}
