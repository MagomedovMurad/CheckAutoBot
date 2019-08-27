using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Models.Captcha
{
    public class CaptchaRequestDataEnvelop
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

       // public object InputData { get; set; }

        public IEnumerable<CaptchaRequestData> CaptchaRequestDataList { get; set; }
    }
}
