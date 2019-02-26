using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Handlers
{
    public class VechiclePassportDataHandler : IDbHandler
    {
        DbQueryExecutor _queryExecutor;
        public VechiclePassportDataHandler(DbQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }
        public ActionType SupportedActionType => ActionType.VechiclePasportData;

        public async Task<Dictionary<string, byte[]>> Get(RequestObject requestObject)
        {
            var cache = await _queryExecutor.GetRequestObjectCacheItem(requestObject.Id);
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
            var text = $"Марка, модель:  {history.Vehicle?.Model}{Environment.NewLine}" +
                       $"Год выпуска: {history.Vehicle?.Year}{Environment.NewLine}" +
                       $"VIN:  {history.Vehicle?.Vin}{Environment.NewLine}" +
                       $"Кузов:  {history.Vehicle?.BodyNumber}{Environment.NewLine}" +
                       $"Цвет: {history.Vehicle?.Color}{Environment.NewLine}" +
                       $"Рабочий объем(см3):  {history.Vehicle?.EngineVolume}{Environment.NewLine}" +
                       $"Мощность(кВт/л.с.):  {history.Vehicle?.PowerKwt ?? "н.д."}/{history.Vehicle?.PowerHp}{Environment.NewLine}" +
                       $"Тип:  {history.Vehicle?.TypeName}{Environment.NewLine}" +
                       $"Категория: {history.Vehicle?.Category}{Environment.NewLine}" +
                       $"Номер двигателя: {history.Vehicle?.EngineNumber}{Environment.NewLine}" +
                       $"Номер ПТС: {history.VehiclePassport?.Number}{Environment.NewLine}" +
                       $"Производитель: {history.VehiclePassport?.CompanyName}";

            return text;
        }
    }
}
