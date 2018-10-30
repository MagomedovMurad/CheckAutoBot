using CheckAutoBot.Exceptions;
using CheckAutoBot.PledgeModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class FnpManager
    {
        private readonly Fnp _fnp;

        public FnpManager()
        {
            _fnp = new Fnp();
        }

        public PledgeResult GetPledges(string vin, string captcha, string sessionId)
        {
            try
            {
                return _fnp.GetPledges(vin, captcha, sessionId);
            }
            catch (WebException ex)
            {
                HttpStatusCode? status = (ex.Response as HttpWebResponse)?.StatusCode;

                if (status == HttpStatusCode.NotFound)
                    return null;
                else if (status == HttpStatusCode.Forbidden)
                    throw new InvalidCaptchaException(captcha);

                throw ex;
            }
        }

        public CaptchaResult GetCaptcha()
        {
            return _fnp.GetCaptcha();
        }
    }
}
