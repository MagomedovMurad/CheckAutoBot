﻿using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.DataSources
{
    public class EaistoDataSource : IDataSourceWithCaptcha
    {
        private EaistoManager _eaistoManager;
        private RucaptchaManager _rucaptchaManager;

        public EaistoDataSource(EaistoManager eaistoManager, RucaptchaManager rucaptchaManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _eaistoManager = eaistoManager;
        }

        public DataType DataType => DataType.DiagnosticCards;

        public int MaxRepeatCount => 2;

        public int Order => 2;

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64, Rucaptcha.LpPingbackUrl);

            return new[] { new CaptchaRequestData(captchaRequest.Id, captchaResult.SessionId, "") };
        }

        public object GetData(object data, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = data as string;
            var captcha = captchaRequestItems.First();

            var lastDiagnosticCard = _eaistoManager.GetLastDiagnosticCard(captcha.Value, null, captcha.SessionId, licensePlate: licensePlate);
            if (lastDiagnosticCard == null)
                return null;

            return lastDiagnosticCard;
        }

    }
}