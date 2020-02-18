using CaptchaSolver.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Infrastructure.Models
{
    public class CaptchaTask
    {
        public CaptchaTask(string id, CaptchaType type, string serializedInputData)
        {
            Id = id;
            CaptchaType = type;
            InputData = serializedInputData;
        }

        public string Id { get; set; }

        public CaptchaType CaptchaType { get; set; }

        public string InputData { get; set; }
    }
}
