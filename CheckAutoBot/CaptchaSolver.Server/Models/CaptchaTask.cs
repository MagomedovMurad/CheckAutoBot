using CaptchaSolver.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Server.Models
{
    public class CaptchaTask<TCaptchaData>
    {
        public string Id { get; set; }

        public CaptchaType CaptchaType { get; set; }

        public TCaptchaData Data { get; set; }
        
        public string Pingback { get; set; }

        public string Result { get; set; }
    }
}
