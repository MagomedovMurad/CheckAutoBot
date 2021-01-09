﻿using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public string Name => "EAISTO_VECHICLE_IDENTIFIERS";

        public DataType DataType => DataType.VechicleIdentifiersEAISTO;

        public int MaxRepeatCount => 1; //2;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;

            var captchaV3 = captchaRequestItems.Single(x => x.Key == "eaistoV3");
            var captchaV2 = captchaRequestItems.Single(x => x.Key == "eaistoV2");

            var eaistoResult = _eaistoManager.GetDiagnosticCards(captchaV3.Value, captchaV2.Value, null, null, licensePlate: licensePlate);
            if (eaistoResult == null)
                return new DataSourceResult(null);

            var historyDCs = eaistoResult.DiagnosticCardsHistory.Select(x => new DiagnosticCard()
            {
                Brand = x.Brand,
                DateFrom = x.DateFrom,
                DateTo = x.DateTo,
                EaistoNumber = x.EaistoNumber,
                FrameNumber = x.FrameNumber,
                LicensePlate = x.LicensePlate,
                Model = x.Model,
                Operator = x.Model,
                Vin = x.Vin
            });
            var currentDC = new DiagnosticCard()
            {
                Brand = eaistoResult.CurrentDiagnosticCard.Brand,
                Model = eaistoResult.CurrentDiagnosticCard.Model,
                DateFrom = eaistoResult.CurrentDiagnosticCard.DateFrom,
                DateTo = eaistoResult.CurrentDiagnosticCard.DateTo,
                EaistoNumber = eaistoResult.CurrentDiagnosticCard.EaistoNumber,
                FrameNumber = eaistoResult.CurrentDiagnosticCard.FrameNumber,
                LicensePlate = eaistoResult.CurrentDiagnosticCard.LicensePlate,
                Operator = eaistoResult.CurrentDiagnosticCard.Operator,
                Vin = eaistoResult.CurrentDiagnosticCard.Vin
            };

            var vechicleIdentifiers = new VechicleIdentifiersData() { FrameNumber = currentDC.FrameNumber, Vin = currentDC.Vin, LicensePlate = currentDC.LicensePlate };
            var allDCs = new List<DiagnosticCard>();
            allDCs.Add(currentDC);
            allDCs.AddRange(historyDCs);

            var relatedData = new RelatedData(new DiagnosticCardsHistory(allDCs), DataType.CurrentDiagnosticCard);
            return new DataSourceResult(vechicleIdentifiers, new[] { relatedData });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequestV3 = _rucaptchaManager.SendReCaptcha3(Eaisto.dataSiteKeyV3, Eaisto.url, Rucaptcha.LpPingbackUrl, 3, "checkNum");
            var captchaRequestV2 = _rucaptchaManager.SendReCaptcha2(Eaisto.dataSiteKeyV2, Eaisto.url, Rucaptcha.LpPingbackUrl);

            return new[] 
            {
                new CaptchaRequestData(captchaRequestV3.Id, null, "eaistoV3"),
                new CaptchaRequestData(captchaRequestV2.Id, null, "eaistoV2")
            };
        }
    }
}
