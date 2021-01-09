﻿using CheckAutoBot.EaistoModels;
using CheckAutoBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
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



        public EaistoResult GetDiagnosticCards(string captchaV3, 
                                               string captchaV2,
                                               string phoneNumber,
                                               string sessionId,
                                               string vin = null,
                                               string licensePlate = null,
                                               string bodyNumber = null,
                                               string chassis = null,
                                               string eaisto = null)
        {
            var eaistoResult = _eaisto.GetDiagnosticCards(captchaV2, captchaV3, vin, licensePlate, bodyNumber, chassis, eaisto);

            if (!string.IsNullOrWhiteSpace(eaistoResult.ErrorMessage))
            {
                if (eaistoResult.ErrorMessage == _notFoundError)
                    return null;

                //if (eaistoResult.ErrorMessage == _invalidCaptchaError)
                //    throw new InvalidCaptchaException(captcha);

                throw new InvalidOperationException(eaistoResult.ErrorMessage);
            }
            else
            {
                return eaistoResult;
            }
        }

        public DiagnosticCard GetLastDiagnosticCard(string licensePlate)
        {
            var lastDk = _eaisto.GetLastDiagnosticCard(licensePlate);
            if(lastDk is null)
                throw new InvalidOperationException("Не удалось получить ДК из Eaisto2DataSource");
            
            if(string.IsNullOrWhiteSpace(lastDk.DateFrom))
                throw new InvalidOperationException("Не удалось получить ДК из Eaisto2DataSource");

            return lastDk;
        }
    }
}
