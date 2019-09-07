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
        private IMessagesSenderController _messagesSenderController;
        private IDataRequestController _dataRequestController;

        public VinCodeController(ICustomLogger logger, 
                                 DbQueryExecutor queryExecutor, 
                                 IDataRequestController dataRequestController,
                                 IMessagesSenderController messagesSenderController)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _keyboardBuilder = new KeyboardBuilder();
            _dataRequestController = dataRequestController;
            _messagesSenderController = messagesSenderController;
        }

        public async Task StartGeneralInfoSearch(string vin, int requestObjectId)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Запущен поиск информации по VIN коду: {vin}");
            _dataRequestController.StartDataSearch(requestObjectId, DataType.GeneralInfo, vin, Callback);
        }

        private async Task Callback(DataRequestResult result)
        {
            var requestObject = _queryExecutor.GetUserRequestObject(result.Id);
            var auto = requestObject as Auto;

            if (!result.IsSuccessfull)
            {
                await _messagesSenderController.SendMessage(auto.UserId, StaticResources.RequestFailedError);
                return;
            }

            var generalInfo = result.DataSourceResult?.Data as GeneralInfo;
            string data;
            var identifier = auto.LicensePlate ?? auto.Vin;

            if (generalInfo is null)
            {
                data = auto.LicensePlate is null ? $"VIN коду" : $"гос. номеру";
                data = $"😕 К сожалению не удалось найти информацию по {data} {identifier}";
                await _messagesSenderController.SendMessage(auto.UserId, data);
                return;
            }

            if (result.DataSourceResult?.RelatedData != null)
                SaveRelatedData(result.Id, result.DataSourceResult.RelatedData);

            var keyboard = _keyboardBuilder.CreateKeyboard(typeof(Auto));
            data = auto.LicensePlate is null ? $"VIN код:" : $"Гос. номер:";
            data = $"✏ {data} {identifier}{Environment.NewLine}" +
                   $"🚗 {generalInfo.Model}, {generalInfo.Year}г.{Environment.NewLine}" +
                   $"⬇ Выберите доступное действие.";

            _messagesSenderController.SendMessage(auto.UserId, data, keyboard: keyboard);
        }

        private void SaveRelatedData(int id, IEnumerable<RelatedData> relatedData)
        {
            foreach (var data in relatedData)
            {
                var json = JsonConvert.SerializeObject(data.Data);
                _queryExecutor.AddRequestObjectCacheItem(id, data.DataType, json);
            }
        }
    }
}
