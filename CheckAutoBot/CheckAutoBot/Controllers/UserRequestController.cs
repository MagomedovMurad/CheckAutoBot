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
        Task HandleUserRequest(int messageId, int userId, RequestType requestType, DateTime date);
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
                _dataRequestController.StartDataSearch(requestId.Value, dataType, auto, Callback);
            }
        }

        private async Task Callback(DataRequestResult dataRequestResult)
        {
            var request = await _queryExecutor.GetUserRequest(dataRequestResult.Id);
            
            //bool requestStatus;

            if (dataRequestResult.IsSuccessfull)
            {
                if (dataRequestResult.DataSourceResult is null)
                {
                    var keyboard = await CreateKeyboard(request.RequestObjectId);
                    _messagesSenderController.SendMessage(request.RequestObject.UserId, StaticResources.UnexpectedError, keyboard: keyboard);
                    _customLogger.WriteToLog(LogLevel.Error, $"Источник данных типа {request.Type} вернул NULL", true);
                    await _queryExecutor.ChangeRequestStatus(request.Id, false);
                }
                else
                {
                    var converter = _dataConverters.Single(x => x.SupportedDataType.Equals(dataRequestResult.DataType));
                    var bags = converter.Convert(dataRequestResult.DataSourceResult.Data);
                    await _queryExecutor.ChangeRequestStatus(request.Id, true);
                    var keyboard = await CreateKeyboard(request.RequestObjectId);

                    foreach (var bag in bags)
                        _messagesSenderController.SendMessage(request.RequestObject.UserId, bag.Message, photo: bag.Picture, keyboard: keyboard);
                }
            }
            else
            {
                var keyboard = await CreateKeyboard(request.RequestObjectId);
                _messagesSenderController.SendMessage(request.RequestObject.UserId, StaticResources.RequestFailedError, keyboard: keyboard);
                await _queryExecutor.ChangeRequestStatus(request.Id, false);
            }

            //await _queryExecutor.ChangeRequestStatus(request.Id, requestStatus);
        }

        private async Task<Keyboard> CreateKeyboard(int requestObjectId)
        {
            var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObjectId).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(typeof(Auto), requestTypes);
        }
    }
}
