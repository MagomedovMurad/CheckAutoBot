using CaptchaSolver.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Infrastructure.Models
{
    public class CaptchaTask
    {
        public CaptchaTask(string id, CaptchaType type, object inputData)
        {
            Id = id;
            CaptchaType = type;
            InputData = inputData;
        }

        public string Id { get; set; }

        public CaptchaType CaptchaType { get; set; }

        public object InputData { get; set; }
    }
}
