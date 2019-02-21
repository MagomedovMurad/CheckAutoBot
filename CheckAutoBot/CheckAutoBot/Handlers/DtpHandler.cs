using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Svg;
using CheckAutoBot.SVG;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class DtpHandler : GibddHandler, IHandler
    {
        private readonly SvgBuilder _svgBuilder;


        public ActionType SupportedActionType => ActionType.Dtp;

        public DtpHandler(GibddManager gibddManager,
                          RucaptchaManager rucaptchaManager) : base(gibddManager, rucaptchaManager)
        {
            _svgBuilder = new SvgBuilder();
        }

        public PreGetResult PreGet()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.RequestPingbackUrl, 3, "check_auto_dtp");
            return new PreGetResult(captchaRequest.Id, null);
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, CaptchaCacheItem cacheItem)
        {
            var auto = requestObject as Auto;

            //var dtpCacheItem = cacheItems.First(x => x.ActionType == ActionType.Dtp);
            var dtpResult = _gibddManager.GetDtp(auto.Vin, cacheItem.CaptchaWord, cacheItem.SessionId);

            return GenerateResponse(dtpResult);
        }

        private Dictionary<string, byte[]> GenerateResponse(DtpResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о дорожно-транспортных происшествиях";
                messages.Add(text, null);
            }
            else
            {
                for (int i = 0; i < result.Accidents?.Count; i++)
                {
                    var accident = result.Accidents.ElementAt(i);
                    byte[] incidentImage = null;
                    var text = AccidentToMessageText(accident, i+1);

                    try
                    {
                        if (accident.DamagePoints.Any())
                            incidentImage = GetAccidentImage(accident.DamagePoints);//_gibddManager.GetIncidentImage(accident.DamagePoints);
                    }
                    catch (WebException ex)
                    {
                        var accidentImageUrl = _gibddManager.GetAccidentImageLink(accident.DamagePoints);
                        text += Environment.NewLine + Environment.NewLine + accidentImageUrl;
                    }

                    messages.Add(text, incidentImage);
                }
            }

            return messages;
        }

        private string AccidentToMessageText(Accident accident, int number)
        {
            return $"{number}. Информация о происшествии №{accident.AccidentNumber} {Environment.NewLine}" +
                    $"Дата и время происшествия: {accident.AccidentDateTime} {Environment.NewLine}" +
                    $"Тип происшествия: {accident.AccidentType} {Environment.NewLine}" +
                    $"Регион происшествия: {accident.RegionName} {Environment.NewLine}" +
                    $"Марка ТС: {accident.VehicleMark} {Environment.NewLine}" +
                    $"Модель ТС: {accident.VehicleModel} {Environment.NewLine}" +
                    $"Год выпуска ТС: {accident.VehicleYear}";
        }

        public byte[] GetAccidentImage(string[] damagePoints)
        {
            DamagePointsType type = DamagePointsType.New;

            if (damagePoints[0].Length == 2)
                type = DamagePointsType.Old;
            else if (damagePoints[0].Length == 3)
                type = DamagePointsType.New;

            var pointsForImage = new List<string>();

            foreach (var point in damagePoints)
            {
                string substr = point;
                if (type == DamagePointsType.New)
                    substr = point.Substring(1, 2);
                int.TryParse(substr, out int pointInt);
                if (ForImages(type, pointInt))
                    pointsForImage.Add(point);
            }

            var svg = _svgBuilder.GenerateDamagePointsSvg(pointsForImage.ToArray(), type);
            return SvgToPngConverter.Convert(svg);
        }

        private bool ForImages(DamagePointsType type, int pointId)
        {
            if (type == DamagePointsType.New)
                return Enumerable.Range(10, 11).Contains(pointId);
            else
                return Enumerable.Range(1, 9).Contains(pointId);
        }


        //private byte[] GetAccidentImage(Accident accident)
        //{
        //    byte[] incidentImage = null;
        //    if (accident.DamagePoints.Any())
        //        incidentImage = _gibddManager.GetIncidentImage(accident.DamagePoints);
        //    return incidentImage;
        //}




    }
}
