using CheckAutoBot.Contracts;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class GibddManager
    {
        private readonly Gibdd _gibdd;

        private readonly string _invalidCaptchaError = "Цифры с картинки введены неверно";
        private readonly string _captchaTimeOutError = "Прошло слишком много времени с момента загрузки картинки, или Ваш брузер не поддеживает cookie";

        public GibddManager()
        {
            _gibdd = new Gibdd();
        }

        public HistoryResult GetHistory(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetHistory(vin, captcha, sessionId);
            if (response.Status == (int)HttpStatusCode.OK)
                return response.RequestResult;

            else if (response.Status == (int)HttpStatusCode.NotFound)
                return null;

            else if (response.Status == (int)HttpStatusCode.Created)
            {
                if (response.Message.Equals(_invalidCaptchaError))
                    throw new InvalidCaptchaException(captcha);
                else if (response.Message.Equals(_captchaTimeOutError))
                    throw new InvalidOperationException(_captchaTimeOutError);
            }

            else if (response.Status == (int)HttpStatusCode.ServiceUnavailable)
                throw new InvalidOperationException();

            throw new Exception(response.Message);
        }

        public DtpResult GetDtp(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetDtp(vin, captcha, sessionId);
            if (response.Status == (int)HttpStatusCode.OK)
            {
                if (response.RequestResult.Accidents.Any())
                    return response.RequestResult;
                else
                    return null;
            }
            else if (response.Status == (int)HttpStatusCode.Created)
            {
                if (response.Message.Equals(_invalidCaptchaError))
                    throw new InvalidCaptchaException(captcha);
                else if (response.Message.Equals(_captchaTimeOutError))
                    throw new InvalidOperationException(_captchaTimeOutError);
            }
            else if (response.Status == (int)HttpStatusCode.ServiceUnavailable)
                throw new InvalidOperationException();

            throw new Exception(response.Message);
        }

        public WantedResult GetWanted(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetWanted(vin, captcha, sessionId);
            if (response.Status == (int)HttpStatusCode.OK)
            {
                if (response.RequestResult.Wanteds.Any())
                    return response.RequestResult;
                else
                    return null;
            }
            else if (response.Status == (int)HttpStatusCode.Created)
            {
                if (response.Message.Equals(_invalidCaptchaError))
                    throw new InvalidCaptchaException(captcha);
                else if (response.Message.Equals(_captchaTimeOutError))
                    throw new InvalidOperationException(_captchaTimeOutError);
            }

            else if (response.Status == (int)HttpStatusCode.ServiceUnavailable)
                throw new InvalidOperationException();

            throw new Exception(response.Message);
        }

        public RestrictedResult GetRestrictions(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetRestriction(vin, captcha, sessionId);
            if (response.Status == (int)HttpStatusCode.OK)
            {
                if (response.RequestResult.Restricteds.Any())
                    return response.RequestResult;
                else
                    return null;
            }
            else if (response.Status == (int)HttpStatusCode.Created)
            {
                if (response.Message.Equals(_invalidCaptchaError))
                    throw new InvalidCaptchaException(captcha);
                else if (response.Message.Equals(_captchaTimeOutError))
                    throw new InvalidOperationException(_captchaTimeOutError);
            }

            else if (response.Status == (int)HttpStatusCode.ServiceUnavailable)
                throw new InvalidOperationException();

            throw new Exception(response.Message);
        }

        public byte[] GetIncidentImage(string[] damagePoints)
        {
            return _gibdd.GetIncidentImage(damagePoints);
        }

        public CaptchaResult GetCaptcha()
        {
            return _gibdd.GetCaptcha();
        }
    }
}
