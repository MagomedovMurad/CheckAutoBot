using CheckAutoBot.Infrastructure.Contracts;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models;
using CheckAutoBot.Infrastructure.Models.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Infrastructure.Converters
{
    public class RestrictedDataConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType.Restricted;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as RestrictedData;
            var bags = new List<ConvertedDataBag>();
            if (data.Accidents is null || data.Accidents.Count() is 0)
            {
                var message = "✅ В базе ГИБДД не найдены сведения о наложении ограничений";
                var bag = new ConvertedDataBag(message);
                bags.Add(bag);
            }
            else
            {
                for (int i = 0; i < data.Accidents.Count(); i++)
                {
                    var message = RestrictedToMessageText(data.Accidents.ElementAt(i), i + 1);
                    var bag = new ConvertedDataBag(message);
                    bags.Add(bag);
                }
            }
            return bags;
        }

        private string RestrictedToMessageText(RestrictedAccident restricted, int number)
        {
            return $"🔒 {number}. Информация о наложении ограничения{Environment.NewLine}" +
                   $"Марка, модель ТС: {restricted.VechicleModel}{Environment.NewLine}" +
                   $"Год выпуска ТС: {restricted.VechicleYear}{Environment.NewLine}" +
                   $"Дата наложения ограничения: {restricted.RestrictedDate}{Environment.NewLine}" +
                   $"Регион инициатора ограничения: {restricted.RegionName}{Environment.NewLine}" +
                   $"Кем наложено ограничение: {restricted.InitiatorType}{Environment.NewLine}" +
                   $"Вид ограничения: {restricted.RestrictedType}{Environment.NewLine}" +
                   $"Основание ограничения: {restricted.RestrictedFoundations}{Environment.NewLine}" +
                   $"Телефон инициатора: {restricted.InitiatorPhone ?? "не указан"}";
        }
    }
}
