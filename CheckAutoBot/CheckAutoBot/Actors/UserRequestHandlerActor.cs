using Akka.Actor;
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
        private IEnumerable<IHandler> _handlers;

        private RsaManager _rsaManager;
        private GibddManager _gibddManager;
        private FnpManager _fnpManager;
        private EaistoManager _eaistoManager;
        private RucaptchaManager _rucaptchaManager;
        private KeyboardBuilder _keyboardBuilder;

        private List<CaptchaCacheItem> _captchaCacheItems = new List<CaptchaCacheItem>();
        private List<CacheItem> _cacheItems = new List<CacheItem>();

        private Dictionary<RequestType, ActionType> _requestTypeToActionType = new Dictionary<RequestType, ActionType>()
        {
            { RequestType.History, ActionType.History },
            { RequestType.Dtp, ActionType.Dtp },
            { RequestType.Restricted, ActionType.Restricted },
            { RequestType.Wanted, ActionType.Wanted },
            { RequestType.Pledge, ActionType.Pledge }
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
            //ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
            ReceiveAsync<StartActionMessage>(x => StartActionMessageHandler(x));
        }

        #region Handlers

        private void InitHandlers()
        {
            _handlers = new List<IHandler>()
            {
                new HistoryHandler(_gibddManager, _rucaptchaManager),
                new DtpHandler(_gibddManager, _rucaptchaManager),
                new RestrictedHandler(_gibddManager, _rucaptchaManager),
                new WantedHandler(_gibddManager, _rucaptchaManager),
                new PledgeHandler(_fnpManager, _rucaptchaManager)
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
                var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == message.CurrentActionType);
                var preGetResults = handler.PreGet();
                AddCaptchaCacheItem(message.RequestId,
                                     preGetResults.CaptchaId,
                                     message.CurrentActionType,
                                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(ex);
                TryExecuteRequestAgain(message.RequestId, message.CurrentActionType);
            }
            catch (Exception ex)
            {
                SendErrorMessage(message.RequestId, StaticResources.UnexpectedError);
                _logger.Error(ex, "Непредвиденная ошибка");
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
                var targetActionType = _requestTypeToActionType[message.RequestType];
                var currentActionType = targetActionType;
                if (auto.Vin == null)
                    currentActionType = ActionType.DiagnosticCard;

                Self.Tell(new StartActionMessage()
                {
                    RequestId = requestId.Value,
                    CurrentActionType = currentActionType,
                });
            }

            return true;
        }

        private async Task<bool> CaptchaResponseMessageHadler(CaptchaResponseMessage message)
        {
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return true;

            captchaItem.CaptchaWord = message.Value;
            var requestCaptchaItems = _captchaCacheItems.Where(x => x.Id == captchaItem.Id);

            var isNotCompleted = requestCaptchaItems.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));

            try
            {
                RucaptchaManager.CheckCaptchaWord(message.Value); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу

                if (!isNotCompleted)
                    await ExecuteGet(captchaItem, requestCaptchaItems);
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.Error($"Неверно решена каптча. Ответ: {icEx.CaptchaWord}");
                _logger.Warn(ex);
                TryExecuteRequestAgain(captchaItem.Id, captchaItem.ActionType);
                isNotCompleted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Непредвиденная ошибка");
                SendErrorMessage(captchaItem.Id, StaticResources.UnexpectedError);
                isNotCompleted = false;
            }

            if (!isNotCompleted)
                RemoveCaptchaCacheItems(captchaItem.Id);

            return true;
        }

        private async Task ExecuteGet(CaptchaCacheItem currentCaptchaItem, IEnumerable<CaptchaCacheItem> requestCaptchaItems)
        {
            var request = await _queryExecutor.GetUserRequest(currentCaptchaItem.Id);
            var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == currentCaptchaItem.ActionType);
            var data = handler.Get(request.RequestObject, requestCaptchaItems);

            await _queryExecutor.MarkRequestCompleted(currentCaptchaItem.Id);

            if (data == null)           //Если нет данных для отправки пользователю
                return;

            var keyboard = await CreateKeyBoard(request.RequestObject).ConfigureAwait(false);

            foreach (var item in data)
            {
                var msg = new SendToUserMessage(keyboard, request.RequestObject.UserId, item.Key, item.Value);
                _senderActor.Tell(msg, Self);
            }
        }

        #endregion Handlers

        #region Helpers
        private void TryExecuteRequestAgain(int requestId, ActionType currentActionType)
        {
            _logger.Debug($"Попытка повторного выполнения запроса с ID: {requestId}. CurrentActionType: {currentActionType}");

            AddOrUpdateCacheItem(requestId, currentActionType);
            var item = _cacheItems.FirstOrDefault(x => x.Id == requestId && x.ActionType == currentActionType);

            _logger.Debug($"В кэше повторных запросов найдена запись с ID запроса: {item.Id}. CurrentActionType: {item.ActionType}. Попыток: {item.AttemptsCount}. Всего записей: {_cacheItems.Count}");

            if (item.AttemptsCount >= 2 )
            {
                if(currentActionType == ActionType.DiagnosticCard)
                {
                    Self.Tell(new StartActionMessage
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.PolicyNumber,
                    });

                    Self.Tell(new StartActionMessage
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.OsagoVechicle,
                    });
                    return;
                }
                SendErrorMessage(requestId, StaticResources.RequestFailedError);
                _logger.Debug($"Из кэша повторных запросов будет удалена запись с ID запроса {item.Id}. CurrentActionType {item.ActionType}. AttemptsCount {item.AttemptsCount}. Всего элементов {_cacheItems.Count}");

                _cacheItems.Remove(item);
                _logger.Debug($"В кэше повторных запросов {_cacheItems.Count} элементов");
                return;
            }

            Self.Tell(new StartActionMessage()
            {
                RequestId = requestId,
                CurrentActionType = currentActionType,
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
            var item = _cacheItems.FirstOrDefault(x => x.Id == requestId && x.ActionType == currentActionType);
            if (item == null)
            {
                _cacheItems.Add(new CacheItem()
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

        private async void SendErrorMessage(int requestId, string text)
        {
            var request = await _queryExecutor.GetUserRequest(requestId);

            var keyboard = await CreateKeyBoard(request.RequestObject);

            var message = new SendToUserMessage(keyboard, request.RequestObject.UserId, text, null);
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
