using CheckAutoBot.Enums;
using CheckAutoBot.Models.RequestedDataCache;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CheckAutoBot.Controllers
{
    public interface IUserRequestController
    {
        Task HandleUserRequest(int messageId, int userId, RequestType requestType, DateTime date);
    }

    public class UserRequestController: IUserRequestController
    {
        private IDataRequestController _dataRequestController;
        private DbQueryExecutor _queryExecutor;
        private IMessagesSenderController _messagesSenderController;
        private KeyboardBuilder _keyboardBuilder;
        private ICustomLogger _customLogger;

        private Dictionary<RequestType, DataType>  _requestTypeToDataType = new Dictionary<RequestType, DataType>()
        {
            { }
        }

        private DataType GetDataType(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.VechiclePassportData:
                    return DataType.VechiclePassportData;
                case RequestType.OwnershipPeriods:
                    return DataType.OwnershipPeriods;
                case RequestType.Dtp:
                    return DataType.Dtp;
                case RequestType.Wanted:
                    return DataType.Wanted;
                case RequestType.Restricted:
                    return DataType.Restricted;
                case RequestType.Pledge:
                    return DataType.Pledge;
                default: throw new InvalidOperationException($"Тип {requestType} не поддерживается");
            }
        }

        public UserRequestController(IDataRequestController dataRequestController,
                                     DbQueryExecutor queryExecutor, 
                                     IMessagesSenderController messagesSenderController,
                                     KeyboardBuilder keyboardBuilder,
                                     ICustomLogger customLogger)
        {
            _dataRequestController = dataRequestController;
            _queryExecutor = queryExecutor;
            _messagesSenderController = messagesSenderController;
            _keyboardBuilder = keyboardBuilder;
            _customLogger = customLogger;
        }

        public async Task HandleUserRequest(int messageId, int userId, RequestType requestType, DateTime date)
        {
            var lastRequestObject = await _queryExecutor.GetLastUserRequestObject(userId);

            var userRequest = new Request()
            {
                RequestObjectId = lastRequestObject.Id,
                Type = requestType
            };

            var requestId = await _queryExecutor.SaveUserRequest(userRequest);
            var dataType = GetDataType(requestType);

            if (lastRequestObject is Auto auto)
            {
                _dataRequestController.StartDataSearch(requestId.Value, dataType, auto.Vin, Callback);
            }
        }

        private async Task Callback(DataRequestResult dataRequestResult)
        {
            var request = await _queryExecutor.GetUserRequest(dataRequestResult.Id);
            var requestTypes = await _queryExecutor.GetExecutedRequestTypes(request.RequestObject.Id).ConfigureAwait(false);
            var keyboard = _keyboardBuilder.CreateKeyboard(typeof(Auto), requestTypes);
            bool requestStatus;

            if (dataRequestResult.IsSuccessfull)
            {
                if (dataRequestResult.DataSourceResult is null)
                {
                    _messagesSenderController.SendMessage(request.RequestObject.UserId, StaticResources.UnexpectedError, keyboard: keyboard);
                    _customLogger.WriteToLog(LogLevel.Error, $"Источник данных типа {request.Type} вернул NULL", true);
                    requestStatus = false;
                }
                else
                {
                    var stringData = dataRequestResult.DataSourceResult.Data.ToString();
                    _messagesSenderController.SendMessage(request.RequestObject.UserId, stringData, keyboard: keyboard);
                    requestStatus = true;
                }
            }
            else
            {
                _messagesSenderController.SendMessage(request.RequestObject.UserId, StaticResources.RequestFailedError, keyboard: keyboard);
                requestStatus = false;
            }

            await _queryExecutor.ChangeRequestStatus(request.Id, requestStatus);
        }
    }
}
