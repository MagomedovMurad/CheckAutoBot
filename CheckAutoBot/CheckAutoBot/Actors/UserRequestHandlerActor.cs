using Akka.Actor;
using CheckAutoBot.Api;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Handlers;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        private ICanSelectActor _actorSelector;
        private DbQueryExecutor _queryExecutor;
        private Random _random;
        private readonly ILogger _logger;
        private IEnumerable<IHttpHandler> _httpHandlers;
        private IEnumerable<IDbHandler> _dbHandlers;

        private RsaManager _rsaManager;
        private GibddManager _gibddManager;
        private FnpManager _fnpManager;
        private EaistoManager _eaistoManager;
        private RucaptchaManager _rucaptchaManager;
        private KeyboardBuilder _keyboardBuilder;

        private List<CaptchaCacheItem> _captchaCacheItems = new List<CaptchaCacheItem>();
        private List<CacheItem> _repeatedRequestsCache = new List<CacheItem>();

        private Dictionary<RequestType, ActionType> _requestTypeToActionType = new Dictionary<RequestType, ActionType>()
        {
            //{ RequestType.History, ActionType.History },
            { RequestType.Dtp, ActionType.Dtp },
            { RequestType.Restricted, ActionType.Restricted },
            { RequestType.Wanted, ActionType.Wanted },
            { RequestType.Pledge, ActionType.Pledge },
            { RequestType.VechiclePassportData, ActionType.VechiclePassportData},
            { RequestType.OwnershipPeriods, ActionType.OwnershipPeriods}
        };
        ICanTell _senderActor;


        public UserRequestHandlerActor(ILogger logger, DbQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
            _logger = logger;

            _rsaManager = new RsaManager();
            _gibddManager = new GibddManager();
            _fnpManager = new FnpManager();
            _rucaptchaManager = new RucaptchaManager();
            _eaistoManager = new EaistoManager();
            _random = new Random();
            _actorSelector = new ActorSelector();
            _keyboardBuilder = new KeyboardBuilder();
            _senderActor = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            InitHandlers();

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
            ReceiveAsync<StartActionMessage>(x => StartActionMessageHandler(x));
        }

        #region Handlers

        private void InitHandlers()
        {
            _httpHandlers = new List<IHttpHandler>()
            {
                //new HistoryHandler(_gibddManager, _rucaptchaManager),
                new DtpHandler(_gibddManager, _rucaptchaManager),
                new RestrictedHandler(_gibddManager, _rucaptchaManager),
                new WantedHandler(_gibddManager, _rucaptchaManager),
                new PledgeHandler(_fnpManager, _rucaptchaManager)
            };

            _dbHandlers = new List<IDbHandler>()
            {
                new VechiclePassportDataHandler(_queryExecutor),
                new OwnershipPeriodsHandler(_queryExecutor)
            };
        }

        /// <summary>
        /// PreGet
        /// </summary>
        /// <param name="message"></param>
        private async Task<bool> StartActionMessageHandler(StartActionMessage message)
        {
            try
            {
                _logger.Debug($"Запрос каптчи для {message.ActionType}. Идентификатор запроса: {message.RequestId}.");

                var handler = _httpHandlers.FirstOrDefault(x => x.SupportedActionType == message.ActionType);
                var preGetResults = handler.PreGet();
                AddCaptchaCacheItem(message.RequestId,
                                     preGetResults.CaptchaId,
                                     message.ActionType,
                                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(ex, $"Ошибка при попытке выполнения запроса каптчи. {Environment.NewLine}" +
                                $"Идентификатор запроса: {message.RequestId}. {Environment.NewLine}");

                await TryExecuteRequestAgain(message.RequestId, message.ActionType);
            }
            catch (Exception ex)
            {
                //Очистка кэша
                RemoveCaptchaCacheItems(message.RequestId);
                RemoveRepeatedRequestsCacheItems(message.RequestId);
                await _queryExecutor.ChangeRequestStatus(message.RequestId, false);

                //Отправка пользователю сообщения о непредвиденной ошибке
                SendErrorMessage(message.RequestId, StaticResources.UnexpectedError);
                _logger.Error(ex, "Непредвиденная ошибка");

                //Отправка сообщения об ошибке мне в вк
                var msg = $"Идентификатор запроса:{message.RequestId}. {Environment.NewLine}" +
                          $"Ошибка: {ex}";
                SendErrorMe(msg);
            }

            return true;
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage message)
        {
            #region SaveRequest

            var lastRequestObject = await _queryExecutor.GetLastUserRequestObject(message.UserId);

            var userRequest = new Request()
            {
                RequestObjectId = lastRequestObject.Id,
                Type = message.RequestType
            };

            var requestId = await _queryExecutor.SaveUserRequest(userRequest);

            #endregion SaveRequest

            if (lastRequestObject is Auto auto)
            {
                var actionType = _requestTypeToActionType[message.RequestType];

                var dataSourceType = GetDataSourceType(actionType);

                if (dataSourceType == DataSourceType.Http)
                    Self.Tell(new StartActionMessage()
                    {
                        RequestId = requestId.Value,
                        ActionType = actionType,
                    });
                else if (dataSourceType == DataSourceType.Db)
                    await StartProcessRequest(actionType, auto, requestId.Value);

            }

            return true;
        }

        private DataSourceType GetDataSourceType(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.VechiclePassportData:
                case ActionType.OwnershipPeriods:
                    return DataSourceType.Db;
                default:
                    return DataSourceType.Http;
            }
        }

        private async Task<bool> CaptchaResponseMessageHadler(CaptchaResponseMessage message)
        {
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
            {
                _logger.Error($"Не надена запись в кэше с идентификатором каптчи {message?.CaptchaId}. Ответ: {message?.Value}");
                return true;
            }

            captchaItem.CaptchaWord = message.Value;
            int requestId = captchaItem.Id;

            try
            {
                RucaptchaManager.CheckCaptchaWord(message.Value); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу
                await ExecuteGet(captchaItem);

                RemoveCaptchaCacheItems(requestId);
                RemoveRepeatedRequestsCacheItems(requestId);
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.Warn(icEx, $"Неверно решена каптча. {Environment.NewLine}" +
                                       $"Ответ: {icEx.CaptchaWord}");
                else
                    _logger.Error(ex, $"Идентификатор запроса: {captchaItem.Id}. Тип действия: {captchaItem.ActionType}");

                _logger.Warn(ex);
                await TryExecuteRequestAgain(captchaItem.Id, captchaItem.ActionType);
            }
            catch (Exception ex)
            {
                RemoveCaptchaCacheItems(requestId);
                RemoveRepeatedRequestsCacheItems(requestId);
                await _queryExecutor.ChangeRequestStatus(requestId, false);

                _logger.Error(ex, "Непредвиденная ошибка");
                SendErrorMessage(captchaItem.Id, StaticResources.UnexpectedError);

                //Отправка сообщения об ошибке мне в вк
                var msg = $"Идентификатор запроса:{requestId}. {Environment.NewLine}" +
                          $"Ошибка: {ex.StackTrace}";
                SendErrorMe(msg);
            }           

            return true;
        }

        private async Task ExecuteGet(CaptchaCacheItem сaptchaItem)
        {
            var request = await _queryExecutor.GetUserRequest(сaptchaItem.Id);
            var handler = _httpHandlers.FirstOrDefault(x => x.SupportedActionType == сaptchaItem.ActionType);
            var data = handler.Get(request.RequestObject, сaptchaItem.CaptchaWord, сaptchaItem.SessionId);

            await _queryExecutor.ChangeRequestStatus(сaptchaItem.Id, true);

            await SendDataToUser(data, request.RequestObject);
        }

        private async Task StartProcessRequest(ActionType actionType, RequestObject requestObject, int requestId)
        {
            var handler = _dbHandlers.FirstOrDefault(x => x.SupportedActionType == actionType);
            var data = await handler.Get(requestObject);

            await _queryExecutor.ChangeRequestStatus(requestId, true);

            await SendDataToUser(data, requestObject);
        }

        private async Task SendDataToUser(Dictionary<string, byte[]> data, RequestObject requestObject)
        {
            if (data == null)           //Если нет данных для отправки пользователю
                return;

            var keyboard = await CreateKeyBoard(requestObject).ConfigureAwait(false);
            foreach (var item in data)
            {
                var msg = new SendToUserMessage(requestObject.UserId, item.Key, item.Value, keyboard);
                _senderActor.Tell(msg, Self);
            }

            if (keyboard.Buttons.Count() == 0)
            {
                var auto = requestObject as Auto;
                var autoData = auto.LicensePlate != null ? auto.LicensePlate : auto.Vin;
                var dataWithType = auto.LicensePlate != null ? $"гос. номеру {autoData}" : $"VIN коду {autoData}";
                var paylink = YandexMoney.GenerateQuickpayUrl(autoData, auto.Id.ToString());
                var text = $"💵 Оплатите запрос по {dataWithType} для выполнения следующего.{Environment.NewLine}" +
                           $"Для оплаты перейдите по ссылке:{Environment.NewLine} " +
                           $"{paylink}";
                var msg = new SendToUserMessage(requestObject.UserId, text);
                _senderActor.Tell(msg, Self);
            }
        }

        #endregion Handlers

        #region Helpers
        private async Task TryExecuteRequestAgain(int requestId, ActionType actionType)
        {
            _logger.Debug($"Попытка повторного выполнения запроса с ID: {requestId}. CurrentActionType: {actionType}");

            AddOrUpdateCacheItem(requestId, actionType);
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestId && x.ActionType == actionType);

            _logger.Debug($"В кэше повторных запросов найдена запись с ID запроса: {item.Id}. CurrentActionType: {item.ActionType}. Попыток: {item.AttemptsCount}. Всего записей: {_repeatedRequestsCache.Count}");

            if (item.AttemptsCount >= 2)
            {
                SendErrorMessage(requestId, StaticResources.RequestFailedError);
                _logger.Debug($"Из кэша повторных запросов будет удалена запись с ID запроса {item.Id}. CurrentActionType {item.ActionType}. AttemptsCount {item.AttemptsCount}. Всего элементов {_repeatedRequestsCache.Count}");

                _repeatedRequestsCache.Remove(item);
                _logger.Debug($"В кэше повторных запросов {_repeatedRequestsCache.Count} элементов");
                await _queryExecutor.ChangeRequestStatus(requestId, false);
                return;
            }

            Self.Tell(new StartActionMessage()
            {
                RequestId = requestId,
                ActionType = actionType,
            });
        }

        private void AddCaptchaCacheItem(int requestId, string captchaId, ActionType currentActionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CaptchaCacheItem()
            {
                Id = requestId,
                CaptchaId = captchaId,
                ActionType = currentActionType,
                SessionId = sessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getPolicyNumberCacheItem);

            _logger.Debug($"В кэш каптчи добавлена запись с RequestId: {requestId}. CaptchaId {captchaId}. currentActionType {currentActionType}. SessionId {sessionId}");
        }

        private void RemoveCaptchaCacheItems(int requestId)
        {
            _captchaCacheItems.RemoveAll(x => x.Id == requestId);
        }

        private void AddOrUpdateCacheItem(int requestId, ActionType currentActionType)
        {
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestId && x.ActionType == currentActionType);
            if (item == null)
            {
                _repeatedRequestsCache.Add(new CacheItem()
                {
                    Id = requestId,
                    ActionType = currentActionType
                });
                _logger.Debug($"В кэш повторных запросов добавлена запись с RequestId {requestId}. CurrentActionType {currentActionType}");
            }
            else
            {
                item.AttemptsCount++;
                _logger.Debug($"В кэше повторных запросов обновлена запись с RequestId {requestId}. CurrentActionType {currentActionType}. AttemptsCount: {item.AttemptsCount}");
            }
        }

        private void RemoveRepeatedRequestsCacheItems(int requestObjectId)
        {
            _repeatedRequestsCache.RemoveAll(x => x.Id == requestObjectId);
        }

        private async void SendErrorMessage(int requestId, string text)
        {
            var request = await _queryExecutor.GetUserRequest(requestId);

            var keyboard = await CreateKeyBoard(request.RequestObject);

            var message = new SendToUserMessage(request.RequestObject.UserId, text, keyboard: keyboard);
            _senderActor.Tell(message, Self);
        }

        private void SendErrorMe(string text)
        {
            var message = new SendToUserMessage(StaticResources.MyUserId, text);
            _senderActor.Tell(message, Self);
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(requestTypes, requestObject.GetType());
        }

        #endregion Helpers

    }
}
