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
    public class WantedConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType. Wanted;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as WantedData;
            var bags = new List<ConvertedDataBag>();
            if (data.Accidents is null || data.Accidents.Count() is 0)
            {
                var message = "✅ В базе ГИБДД не найдены сведения о розыске транспортного средства";
                var bag = new ConvertedDataBag(message);
                bags.Add(bag);
            }
            else
            {
                for (int i = 0; i < data.Accidents.Count(); i++)
                {
                    var message = WantedToMessageText(data.Accidents.ElementAt(i), i + 1);
                    var bag = new ConvertedDataBag(message);
                    bags.Add(bag);
                }
            }
            return bags;
        }

        private string WantedToMessageText(WantedAccident accident, int number)
        {
            return $"🕵 {number}. Информация о постановке в розыск{Environment.NewLine}" +
                   $"Марка, модель: {accident.VechicleModel}{Environment.NewLine}" +
                   $"Год выпуска: {accident.VechicleYear}{Environment.NewLine}" +
                   $"Дата объявления в розыск: {accident.Date}{Environment.NewLine}" +
                   $"Регион инициатора розыска: {accident.RegionIniciator}";
        }
    }
}
