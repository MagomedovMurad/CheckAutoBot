using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Handlers
{
    public class OwnershipPeriodsHandler : IDbHandler
    {
        DbQueryExecutor _queryExecutor;
        public OwnershipPeriodsHandler(DbQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        public ActionType SupportedActionType => ActionType.OwnershipPeriods;

        public async Task<Dictionary<string, byte[]>> Get(RequestObject auto)
        {
            var cache = await _queryExecutor.GetRequestObjectCacheItem(auto.Id);
            var historyResult = JsonConvert.DeserializeObject<HistoryResult>(cache.Data);

            return GenerateResponse(historyResult);
        }

        private Dictionary<string, byte[]> GenerateResponse(HistoryResult result)
        {
            string text = HistoryToMessageText(result);
            return new Dictionary<string, byte[]>() { { text, null } };
        }

        private string HistoryToMessageText(HistoryResult history)
        {
            var periods = history.OwnershipPeriodsEnvelop?.OwnershipPeriods;
            string text = string.Empty;


            for (int i = 0; i < periods?.Count; i++)
            {
                var period = periods.ElementAt(i);
                var ownerType = period.OwnerType == OwnerType.Natural ? "Физическое лицо" : "Юридическое лицо";
                var stringDateTo = period.To.ToString("dd.MM.yyyy");
                var dateTo = stringDateTo == "01.01.0001" ? "настоящее время" : stringDateTo;

                string ownerPeriod = $"{Environment.NewLine}" +
                    $"{Environment.NewLine}{i + 1}. {ownerType}{Environment.NewLine}" +
                                     $"c: {period.From.ToString("dd.MM.yyyy")}{Environment.NewLine}" +
                                     $"по: {dateTo}{Environment.NewLine}" +
                                     $"Последняя операция: {period.LastOperation}";
                text += ownerPeriod;
            }

            return text;
        }
    }
}
