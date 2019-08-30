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
    public class PledgeConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType.Pledge;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as PledgeData;
            var bags = new List<ConvertedDataBag>();
            if (data.Accidents.Count() is 0)
            {
                var message = "✅ В базе ФНП не найдены сведения о нахождении транспортного средства в залоге";
                var bag = new ConvertedDataBag(message);
                bags.Add(bag);
            }
            else
            {
                for (int i = 0; i < data.Accidents.Count(); i++)
                {
                    var message = PledgeToText(data.Accidents.ElementAt(i), i + 1);
                    var bag = new ConvertedDataBag(message);
                    bags.Add(bag);
                }
            }
            return bags;
        }
        private string PledgeToText(PledgeAccident pledge, int number)
        {
            var text = $"📃 {number}. Уведомление о возникновении залога №{pledge.Id} {Environment.NewLine}";
            text += $"Дата регистрации: {pledge.RegistrationDate.ToString("dd.MM.yyyy H:mm:ss")}{Environment.NewLine}";
            text += $"Залогодатель: {string.Join(Environment.NewLine, pledge.Pledgors.Select(x => x.Name))} {Environment.NewLine}";
            text += $"Залогодержатель: {string.Join(Environment.NewLine, pledge.Pledgees.Select(x => x.Name))}";
            text += Environment.NewLine;

            return text;
        }
    }
}
