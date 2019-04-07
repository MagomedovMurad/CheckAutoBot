using CheckAutoBot.Exceptions;
using CheckAutoBot.FnpModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class FnpManager
    {
        private readonly string _invalidCaptchaError = "Captcha token required";
        private readonly Fnp _fnp;

        public FnpManager()
        {
            _fnp = new Fnp();
        }

        public PledgeResponse GetPledges(string vin, string captcha, string sessionId)
        {

            var response = _fnp.GetPledges(vin, captcha, sessionId);

            if (response is null)
                throw new InvalidOperationException("Response is null");

            if (!string.IsNullOrEmpty(response.Error))
            {
                if (response.Message == _invalidCaptchaError)
                    throw new InvalidCaptchaException(captcha);
                throw new InvalidOperationException($"Error: {response?.Message}. Status code: {response?.Status}. Message: {response?.Message}");
            }

            return response;
        }

        public CaptchaResult GetCaptcha()
        {
            return _fnp.GetCaptcha();
        }
    }
}
