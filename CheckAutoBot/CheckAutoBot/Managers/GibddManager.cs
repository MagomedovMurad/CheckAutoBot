using CheckAutoBot.Contracts;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Svg;
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
        private readonly string _invalidCaptchaError = "Проверка с помощью Google reCaptcha v3 не была пройдена, повторите попытку.";

        public GibddManager()
        {
            _gibdd = new Gibdd();
        }

        public HistoryResult GetHistory(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetHistory(vin, captcha, sessionId);

            if(response == null)
                throw new InvalidOperationException("History response is null");

            if (response.Status == (int)HttpStatusCode.OK)
                return response.RequestResult;

            else if (response?.Status == (int)HttpStatusCode.NotFound)
                return null;

            if (response.Message.Equals(_invalidCaptchaError))
                throw new InvalidCaptchaException(captcha);

            throw new InvalidOperationException(response?.Message);
        }

        public DtpResult GetDtp(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetDtp(vin, captcha, sessionId);

            if(response == null)
                throw new InvalidOperationException("Dtp response is null");

            if (response.Status == (int)HttpStatusCode.OK)
            {
                if (response.RequestResult.Accidents.Any())
                    return response.RequestResult;
                else
                    return null;
            }

            if(response?.Message?.Equals(_invalidCaptchaError) == true)
                throw new InvalidCaptchaException(captcha);

            throw new InvalidOperationException(response?.Message);
        }

        public WantedResult GetWanted(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetWanted(vin, captcha, sessionId);

            if (response == null)
                throw new InvalidOperationException("Wanted response is null");

            if (response.RequestResult.Wanteds.Any())
                return response.RequestResult;

            if (response.Status == (int)HttpStatusCode.OK)
            {
                if (response.RequestResult.Wanteds.Any())
                    return response.RequestResult;
                else
                    return null;
            }

            if (response.Message.Equals(_invalidCaptchaError))
                throw new InvalidCaptchaException(captcha);

            throw new InvalidOperationException(response.Message);
        }

        public RestrictedResult GetRestrictions(string vin, string captcha, string sessionId)
        {
            var response = _gibdd.GetRestriction(vin, captcha, sessionId);

            if (response == null)
                throw new InvalidOperationException("Restrictions response is null");

            if (response.Message?.Equals(_invalidCaptchaError) == true)
                throw new InvalidCaptchaException(captcha);

            if (response.RequestResult?.Restricteds?.Any() == true)
                return response.RequestResult;
            else if(response.RequestResult?.Restricteds?.Any() == false)
                return null;

            throw new InvalidOperationException(response.Message);
        }

        public byte[] GetIncidentImage(string[] damagePoints)
        {
            return _gibdd.GetIncidentImage(damagePoints);
        }

        
        public string GetAccidentImageLink(string[] damagePoints)
        {
            return _gibdd.GetAccidentImageLink(damagePoints);
        }

        public CaptchaResult GetCaptcha()
        {
            return _gibdd.GetCaptcha();
        }
    }
}
