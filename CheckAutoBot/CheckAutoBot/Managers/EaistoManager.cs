using CheckAutoBot.EaistoModels;
using CheckAutoBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class EaistoManager
    {
        private readonly Eaisto _eaisto;

        private const string _invalidCaptchaError = "Пожалуйста введите код, указанный на картинке";
        //private const string _serverErrorWithSorry = "Извините. Во время поиска произошла ошибка. Попробуйте, пожалуйста, позже.";
        //private const string _serverError = "Во время поиска произошел сбой. Пожалуйста повторите попытку позже.";
        private const string _notFoundError = "По Вашему запросу ничего не найдено";

        public EaistoManager()
        {
            _eaisto = new Eaisto();
        }

        public DiagnosticCard GetDiagnosticCard(string captcha,
                                      string phoneNumber,
                                      string sessionId,
                                      string vin = null,
                                      string licensePlate = null,
                                      string bodyNumber = null,
                                      string chassis = null,
                                      string eaisto = null)
        {
            var diagnosticCard = _eaisto.GetDiagnosticCard(captcha, sessionId, vin, licensePlate, bodyNumber, chassis, eaisto);

            if (string.IsNullOrWhiteSpace(diagnosticCard.ErrorMessage) &&
                !string.IsNullOrWhiteSpace(diagnosticCard.Vin))
                return diagnosticCard;

            else if (diagnosticCard.ErrorMessage == _notFoundError)
                return null;

            else if (diagnosticCard.ErrorMessage == _invalidCaptchaError)
                throw new InvalidCaptchaException(captcha);

            throw new InvalidOperationException(diagnosticCard.ErrorMessage);
        }

        public CaptchaResult GetCaptcha()
        {
            return _eaisto.GetCaptcha();
        }

    }
}
