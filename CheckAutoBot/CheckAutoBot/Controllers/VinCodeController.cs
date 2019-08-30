using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;
using CheckAutoBot.Models.RequestedDataCache;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;

namespace CheckAutoBot.Controllers
{
    public interface IVinCodeController
    {
        Task StartGeneralInfoSearch(string vin, int requestObjectId);
    }

    public class VinCodeController: IVinCodeController
    {
        private readonly ICustomLogger _logger;
        private DbQueryExecutor _queryExecutor;
        private KeyboardBuilder _keyboardBuilder;
        private MessagesSenderController _messagesSenderController;
        private DataRequestController _dataRequestController;

        public VinCodeController(ICustomLogger logger, 
                                 DbQueryExecutor queryExecutor, 
                                 DataRequestController dataRequestController)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _keyboardBuilder = new KeyboardBuilder();
            _dataRequestController = dataRequestController;
        }

        public async Task StartGeneralInfoSearch(string vin, int requestObjectId)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Запущен поиск информации по VIN коду: {vin}");
            _dataRequestController.StartDataSearch(requestObjectId, DataType.GeneralInfo, vin, Callback);
        }

        private async Task Callback(DataRequestResult result)
        {
            var requestObject = await _queryExecutor.GetUserRequestObject(result.Id);
            var generalInfo = result.DataSourceResult.Data as GeneralInfo;
            SaveRelatedData(result.Id, result.DataSourceResult.RelatedData);

            var auto = requestObject as Auto;
            var identifier = auto.LicensePlate ?? auto.Vin;
            string data;

            if (generalInfo is null)
            {
                data = auto.LicensePlate is null ? $"гос. номеру" : $"VIN коду";
                data = $"😕 К сожалению не удалось найти информацию по {data} {identifier}";
                await _messagesSenderController.SendMessage(auto.UserId, data);
                return;
            }

            var keyboard = _keyboardBuilder.CreateKeyboard(typeof(Auto));
            data = auto.LicensePlate is null ? $"Гос. номер:" : $"VIN код:";
            data = $"✏ {data} {identifier}{Environment.NewLine}" +
                   $"🚗 {generalInfo.Model}, {generalInfo.Year}г.{Environment.NewLine}" +
                   $"⬇ Выберите доступное действие.";

            _messagesSenderController.SendMessage(auto.Id, data, keyboard: keyboard);
        }

        private void SaveRelatedData(int id, IEnumerable<RelatedData> relatedData)
        {
            foreach (var data in relatedData)
            {
                var json = JsonConvert.SerializeObject(data);
                _queryExecutor.AddRequestObjectCacheItem(id, data.DataType, json);
            }
        }
    }
}
