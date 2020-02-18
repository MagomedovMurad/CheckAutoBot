using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Infrastructure.Models
{
    public class RecaptchaV3Data
    {
        public RecaptchaV3Data(string pageUrl, string action, string googleKey)
        {
            PageUrl = pageUrl;
            Action = action;
            GoogleKey = googleKey;
        }
        public string PageUrl { get; set; }

        public string Action { get; set; }

        public string GoogleKey { get; set; }
    }
}
