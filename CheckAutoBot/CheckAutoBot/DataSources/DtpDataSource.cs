using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Storage;
using CheckAutoBot.Svg;
using CheckAutoBot.SVG;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace CheckAutoBot.DataSources
{
    public class DtpDataSource : IDataSourceWithCaptcha
    {
        private RucaptchaManager _rucaptchaManager;
        private GibddManager _gibddManager;
        private SvgBuilder _svgBuilder;
        private ICustomLogger _customLogger;

        public DtpDataSource(RucaptchaManager rucaptchaManager, GibddManager gibddManager, SvgBuilder svgBuilder, ICustomLogger customLogger)
        {
            _rucaptchaManager = rucaptchaManager;
            _gibddManager = gibddManager;
            _svgBuilder = svgBuilder;
            _customLogger = customLogger;
        }

        public string Name => "GIBDD_DTP";

        public DataType DataType => DataType.Dtp;

        public int MaxRepeatCount => 3;

        public int Order => 1;


        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var auto = inputData as Auto;
            var captchaRequestData = captchaRequestItems.Single();
            var dtpResult = _gibddManager.GetDtp(auto.Vin, captchaRequestData.CaptchaId, captchaRequestData.SessionId);

            if (dtpResult is null)
                return new DataSourceResult(new DtpData());

            var accidents = new List<DtpAccident>();

            foreach (var accident in dtpResult.Accidents)
            {
                var picture = GetAccidentPicture(accident);

                var dtAccident = new DtpAccident()
                {
                    AccidentDateTime = accident.AccidentDateTime,
                    AccidentNumber = accident.AccidentNumber,
                    AccidentType = accident.AccidentType,
                    RegionName = accident.RegionName,
                    VehicleDamageState = accident.VehicleDamageState,
                    VehicleMark = accident.VehicleMark,
                    VehicleModel = accident.VehicleModel,
                    VehicleYear = accident.VehicleYear,
                    Picture = picture
                };

                accidents.Add(dtAccident);
            }

            return new DataSourceResult(new DtpData(){ Accidents = accidents });
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequest = _rucaptchaManager.SendReCaptcha3(Gibdd.dataSiteKey, Gibdd.url, Rucaptcha.RequestPingbackUrl, 3, "check_auto_dtp");
            return new[] { new CaptchaRequestData(captchaRequest.Id, null, string.Empty) };
        }


        private byte[] GetAccidentPicture(Accident accident)
        {
            if (!accident.DamagePoints.Any())
            {
                _customLogger.WriteToLog(LogLevel.Debug, "Отсутствуют точки повреждений");
                return null;
            }
            try
            {
                return GetAccidentImage(accident.DamagePoints);
            }
            catch (Exception ex)
            {
                _customLogger.WriteToLog(LogLevel.Error, $"Ошибка при конвертации SVG в PNG (№{accident?.AccidentNumber}): {ex}", true);
                return null;
            }
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
            if (!pointsForImage.Any())
                return null;

            var svg = _svgBuilder.GenerateDamagePointsSvg(pointsForImage.ToArray(), type);
            return SvgToPngConverter.Convert(svg);
        }

        private bool ForImages(DamagePointsType type, int pointId)
        {
            if (type == DamagePointsType.New)
                return pointId >= 10;
            else
                return Enumerable.Range(1, 9).Contains(pointId);
        }
    }
}
