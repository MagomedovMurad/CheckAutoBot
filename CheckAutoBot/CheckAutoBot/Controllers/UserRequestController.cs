using CheckAutoBot.Enums;
using CheckAutoBot.Models.RequestedDataCache;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Linq;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Contracts;
using CheckAutoBot.Vk.Api.MessagesModels;

namespace CheckAutoBot.Controllers
{
    public interface IUserRequestController
    {
        void HandleUserRequest(int messageId, int userId, RequestType requestType, DateTime date);
    }

    public class UserRequestController: IUserRequestController
    {
        private IDataRequestController _dataRequestController;
        private DbQueryExecutor _queryExecutor;
        private IMessagesSenderController _messagesSenderController;
        private KeyboardBuilder _keyboardBuilder;
        private ICustomLogger _customLogger;
        private IEnumerable<IDataConverter> _dataConverters;

        private Dictionary<RequestType, DataType> _requestTypeToDataType = new Dictionary<RequestType, DataType>()
        {
            { RequestType.VechiclePassportData, DataType.VechiclePassportData},
            { RequestType.OwnershipPeriods, DataType.OwnershipPeriods},
            { RequestType.Dtp, DataType.Dtp},
            { RequestType.Wanted, DataType.Wanted},
            { RequestType.Restricted, DataType.Restricted},
            { RequestType.Pledge, DataType.Pledge}
        };
        

        public UserRequestController(IDataRequestController dataRequestController,
                                     DbQueryExecutor queryExecutor, 
                                     IMessagesSenderController messagesSenderController,
                                     //KeyboardBuilder keyboardBuilder,
                                     ICustomLogger customLogger,
                                     IDataConverter[] dataConverters)
        {
            _dataRequestController = dataRequestController;
            _queryExecutor = queryExecutor;
            _messagesSenderController = messagesSenderController;
            _keyboardBuilder = new KeyboardBuilder();
            _customLogger = customLogger;
            _dataConverters = dataConverters;
        }

        private DataType GetDataType(RequestType requestType)
        {
            return _requestTypeToDataType[requestType];
        }

        private RequestType GetRequestType(DataType dataType)
        {
            return _requestTypeToDataType.Single(x => x.Value == dataType).Key;
        }

        public void HandleUserRequest(int messageId, int userId, RequestType requestType, DateTime date)
        {
            var lastRequestObject = _queryExecutor.GetLastUserRequestObject(userId);

            var userRequest = new Request()
            {
                RequestObjectId = lastRequestObject.Id,
                Type = requestType
            };

            var requestId = _queryExecutor.SaveUserRequest(userRequest);
            var dataType = GetDataType(requestType);

            if (lastRequestObject is Auto auto)
            {
                _dataRequestController.StartDataSearch(requestId.Value, dataType, auto, Callback);
            }
        }

        private async Task Callback(DataRequestResult dataRequestResult)
        {
            try
            {
                var request = _queryExecutor.GetUserRequest(dataRequestResult.Id);

                bool requestState;
                Keyboard keyboard = null;
                Action sendAction = null;

                if (dataRequestResult.IsSuccessfull)
                {
                    if (dataRequestResult.DataSourceResult is null)
                    {
                        sendAction = new Action(() => SendMessageToUser(request.RequestObject.UserId, StaticResources.UnexpectedError, keyboard: keyboard));
                        requestState = false;
                        _customLogger.WriteToLog(LogLevel.Error, $"Источник данных типа {request.Type} вернул NULL", true);
                    }
                    else
                    {
                        var converter = _dataConverters.Single(x => x.SupportedDataType.Equals(dataRequestResult.DataType));
                        var bags = converter.Convert(dataRequestResult.DataSourceResult.Data);
                        requestState = true;
                        sendAction = new Action(() =>
                        {
                            foreach (var bag in bags)
                                SendMessageToUser(request.RequestObject.UserId, bag.Message, picture: bag.Picture, keyboard: keyboard);
                        });
                    }
                }
                else
                {
                    sendAction = new Action(() => SendMessageToUser(request.RequestObject.UserId, StaticResources.RequestFailedError, keyboard: keyboard));
                    requestState = false;
                }

                _queryExecutor.ChangeRequestStatus(request.Id, requestState);
                keyboard = CreateKeyboard(request.RequestObjectId);
                sendAction();
            }
            catch (Exception ex)
            {
                var message = "Произошла ошибка при обработке полученных данных от источников (UserRequestController): " + ex;
                _customLogger.WriteToLog(LogLevel.Error, message, true);
            }
        }

        private void SendMessageToUser(int userId, string message, byte[] picture = null, Keyboard keyboard = null)
        {
            _messagesSenderController.SendMessage(userId, message, picture, keyboard);
        }

        private Keyboard CreateKeyboard(int requestObjectId)
        {
            var requestTypes = _queryExecutor.GetExecutedRequestTypes(requestObjectId);
            return _keyboardBuilder.CreateKeyboard(typeof(Auto), requestTypes);
        }
    }
}
