using CheckAutoBot.EaistoModels;
using CheckAutoBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class EaistoManager
    {
        private const string _invalidCaptchaError = "Пожалуйста введите код, указанный на картинке";
        private const string _notFoundError = "По Вашему запросу ничего не найдено";


        private readonly Eaisto _eaisto;

        public DiagnosticCard GetDiagnosticCard(string captcha,
                                      string phoneNumber,
                                      string sessionId,
                                      string vin = null,
                                      string licensePlate = null,
                                      string bodyNumber = null,
                                      string chassis = null,
                                      string eaisto = null)
        {
            var diagnosticCard = _eaisto.GetDiagnosticCard(captcha, phoneNumber, sessionId, vin, licensePlate, bodyNumber, chassis, eaisto);

            if (string.IsNullOrEmpty(diagnosticCard.ErrorMessage))
                return diagnosticCard;
            else if (diagnosticCard.ErrorMessage == _notFoundError)
                return null;
            else if (diagnosticCard.ErrorMessage == _invalidCaptchaError)
                throw new InvalidCaptchaException(captcha);

            throw new Exception(diagnosticCard.ErrorMessage);
        }

        public CaptchaResult GetCaptcha()
        {
            return _eaisto.GetCaptcha();
        }

    }
}
