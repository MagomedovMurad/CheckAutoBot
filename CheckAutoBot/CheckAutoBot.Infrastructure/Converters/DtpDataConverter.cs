using CheckAutoBot.Infrastructure.Contracts;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models;
using CheckAutoBot.Infrastructure.Models.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CheckAutoBot.Infrastructure.Converters
{
    public class DtpDataConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType.Dtp;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as DtpData;

            var bags = new List<ConvertedDataBag>();
            if (data.Accidents is null || data.Accidents.Count() is 0)
            {
                var message = "✅ В базе ГИБДД не найдены сведения о дорожно-транспортных происшествиях";
                var bag = new ConvertedDataBag(message);
                bags.Add(bag);
            }
            else
            {
                for (int i = 0; i < data.Accidents.Count(); i++)
                {
                    var accident = data.Accidents.ElementAt(i);
                    var message = AccidentToMessageText(accident, i + 1);

                    var bag = new ConvertedDataBag(message, accident.Picture);
                    bags.Add(bag);
                }
            }

            return bags;
        }

        private string AccidentToMessageText(DtpAccident accident, int number)
        {
            return $"💥 {number}. Информация о происшествии №{accident.AccidentNumber} {Environment.NewLine}" +
                    $"Дата и время происшествия: {accident.AccidentDateTime} {Environment.NewLine}" +
                    $"Тип происшествия: {accident.AccidentType} {Environment.NewLine}" +
                    $"Регион происшествия: {accident.RegionName} {Environment.NewLine}" +
                    $"Марка ТС: {accident.VehicleMark} {Environment.NewLine}" +
                    $"Модель ТС: {accident.VehicleModel} {Environment.NewLine}" +
                    $"Год выпуска ТС: {accident.VehicleYear}";
        }

        //public byte[] GetAccidentImage(string[] damagePoints)
        //{
        //    DamagePointsType type = DamagePointsType.New;

        //    if (damagePoints[0].Length == 2)
        //        type = DamagePointsType.Old;
        //    else if (damagePoints[0].Length == 3)
        //        type = DamagePointsType.New;

        //    var pointsForImage = new List<string>();

        //    foreach (var point in damagePoints)
        //    {
        //        string substr = point;
        //        if (type == DamagePointsType.New)
        //            substr = point.Substring(1, 2);
        //        int.TryParse(substr, out int pointInt);
        //        if (ForImages(type, pointInt))
        //            pointsForImage.Add(point);
        //    }
        //    if (!pointsForImage.Any())
        //        return null;

        //    var svg = _svgBuilder.GenerateDamagePointsSvg(pointsForImage.ToArray(), type);
        //    return SvgToPngConverter.Convert(svg);
        //}

        //private bool ForImages(DamagePointsType type, int pointId)
        //{
        //    if (type == DamagePointsType.New)
        //        return pointId >= 10;
        //    else
        //        return Enumerable.Range(1, 9).Contains(pointId);
        //}
    }
}
