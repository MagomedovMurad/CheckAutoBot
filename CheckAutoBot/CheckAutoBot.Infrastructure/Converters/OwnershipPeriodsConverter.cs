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
    public class OwnershipPeriodsConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType.OwnershipPeriods;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as OwnershipPeriodPata;

            var message = ConvertToStringMessage(data);
            return new[] { new ConvertedDataBag()
            {
                Message = message
            }};
        }

        private string ConvertToStringMessage(OwnershipPeriodPata data)
        {
            var periods = data.OwnershipPeriods;
            string text = string.Empty;


            for (int i = 0; i < periods?.Count; i++)
            {
                var period = periods.ElementAt(i);
                var ownerType = period.OwnerType == OwnerType.Natural ? "🚶 Физическое лицо" : "🏢 Юридическое лицо";
                var stringDateTo = period.DateTo.ToString("dd.MM.yyyy");
                var dateTo = stringDateTo == "01.01.0001" ? "настоящее время" : stringDateTo;

                string ownerPeriod = $"{Environment.NewLine}" +
                    $"{Environment.NewLine}{i + 1}. {ownerType}{Environment.NewLine}" +
                                     $"c: {period.DateFrom.ToString("dd.MM.yyyy")}{Environment.NewLine}" +
                                     $"по: {dateTo}{Environment.NewLine}" +
                                     $"Последняя операция: {period.LastOperation}";
                text += ownerPeriod;
            }

            return text;
        }
    }
}
