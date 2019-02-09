using CheckAutoBot.Exceptions;
using CheckAutoBot.RsaModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class RsaManager
    {
        private readonly Rsa _rsa;
        private readonly string _policyNotFound = "Сведения о полисе ОСАГО с указанными серией и номером не найдены";

        public RsaManager()
        {
            _rsa = new Rsa();
        }

        public PolicyResponse GetPolicy(string captcha, DateTime date, string lp = "", string vin = "", string bodyNumber = "", string chassisNumber = "")
        {
            var response = _rsa.GetPolicy(captcha, date, lp, vin, bodyNumber, chassisNumber);

            if (response == null)
                throw new InvalidOperationException("Policy response is null");

            if (!response.ValidCaptcha)
                throw new InvalidCaptchaException(captcha);

            if (response.ErrorId == 0)
                return response;

            else if (response.ErrorId == 7002 || response.ErrorId == 7005)
                return null;

            throw new InvalidOperationException(response.ErrorMessage);
        }

        public VechicleResponse GetVechicleInfo(string serial, string number, DateTime date, string captcha)
        {
            var response = _rsa.GetVechicleInfo(serial, number, date, captcha);

            if (response == null)
                throw new InvalidOperationException("Vechicle info response is null");

            if (!response.ValidCaptcha)
                throw new InvalidCaptchaException(captcha);

            if (response.ErrorId == 0)
            {
                if (response.WarningMessage == null)
                    return response;
                else if (!response.WarningMessage.Equals(_policyNotFound))
                    return null;
            }

            else if (response.ErrorId == 7002 || response.ErrorId == 7005)
                return null;

            throw new Exception($"ErrorId: {response?.ErrorId}. " +
                                $"Error: {response?.ErrorMessage}. " +
                                $"Warning: {response?.WarningMessage}");

        }
    }
}
