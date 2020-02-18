using CaptchaSolver.Infrastructure.Enums;
using CaptchaSolver.Server.Enums;
using System;

namespace CaptchaSolver.Server.Models
{
    public class CaptchaTasksCacheItem
    {
        public string Id { get; set; }

        public CaptchaType CaptchaType { get; set; }

        public CaptchaTaskState TaskState { get; set; }
        
        public string Pingback { get; set; }

        public string SerializedInputData { get; set; }

        public DateTime DateTime { get; set; }

        public string Result { get; set; }
    }
}
