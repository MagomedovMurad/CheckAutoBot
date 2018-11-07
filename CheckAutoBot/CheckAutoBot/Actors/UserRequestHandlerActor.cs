using Akka.Actor;
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
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
            ReceiveAsync<StartActionMessage>(x => StartActionMessageHandler(x));
        }

        #region Handlers

        private void InitHandlers()
        {
            _handlers = new List<IHandler>()
            {
                new VinByDiagnosticCardHandler(Self, _queryExecutor),
                new PolicyNumberHandler(_rsaManager, _rucaptchaManager, Self, _queryExecutor),
                new OsagoVechicleHandler(_rsaManager, _rucaptchaManager, Self, _queryExecutor),
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
                //AddOrUpdateCacheItem(message.RequestId, message.CurrentActionType);
                var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == message.CurrentActionType);
                var preGetResults = handler.PreGet();
                AddCaptchaCacheItem(message.RequestId,
                                     preGetResults.CaptchaId,
                                     message.CurrentActionType,
                                     message.TargetActionType,
                                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(ex);
                TryExecuteRequestAgain(message.RequestId, message.CurrentActionType,message.TargetActionType);
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
                    currentActionType = ActionType.VinByDiagnosticCard;

                Self.Tell(new StartActionMessage()
                {
                    RequestId = requestId.Value,
                    CurrentActionType = currentActionType,
                    TargetActionType = targetActionType
                });
            }

            return true;
        }

        private async Task<bool> UserInputDataMessageHandler(UserInputDataMessage message)
        {
            try
            {
                RequestObject data;

                switch (message.Type)
                {
                    #region VIN
                    case InputDataType.Vin:
                        data = new Auto
                        {
                            Vin = message.Data,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion VIN

                    #region LicensePlate
                    case InputDataType.LicensePlate:
                        data = new Auto
                        {
                            LicensePlate = message.Data,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;

                    #endregion LicensePlate

                    #region FullName
                    case InputDataType.FullName:

                        string[] personData = message.Data.Split(' ');
                        string lastName = personData[0].Replace('_', ' '); //Фамилия
                        string firstName = personData[1].Replace('_', ' '); //Имя
                        string middleName = personData[2].Replace('_', ' '); //Отчество
                        data = new Person
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            MiddleName = middleName,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion FullName

                    default:
                        throw new InvalidOperationException($"Не найден обработчик для типа {message.Type}");
                        break;
                }

                await _queryExecutor.AddRequestObject(data);

                var text = $"{message.Data}{Environment.NewLine}Выберите доступные действия...";
                var keyboard = await CreateKeyBoard(data).ConfigureAwait(false);
                var msg = new SendToUserMessage(keyboard, message.UserId, text, null);

                _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(msg, Self);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> CaptchaResponseMessageHadler(CaptchaResponseMessage message)
        {
            _logger.Debug($"Получена каптча с ID: {message.CaptchaId}. Значение: {message.Value}");
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            _logger.Debug($"Найден элемент в кэше с ID: {captchaItem.CaptchaId}. CurrentActionType: {captchaItem.CurrentActionType}. RequestId: {captchaItem.RequestId}. Всего в кэше элементов: {_captchaCacheItems.Count}");
            if (captchaItem == null)
                _logger.Error($"В кэше не найдена запись с идентификатором каптчи {message.CaptchaId}");

            captchaItem.CaptchaWord = message.Value;
            var requestCptchaItems = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);

            _logger.Debug($"Все записи в кэше с RequestId: {captchaItem.RequestId}");
            foreach (var item in requestCptchaItems)
            {
                _logger.Debug($"CaptchaId: {item.CaptchaId}. CurrentActionType: {item.CurrentActionType}. TargetActionType: {item.TargetActionType}. CaptchaWord: {item.CaptchaWord}. SessionId: {item.SessionId}");
            }

            var isNotCompleted = requestCptchaItems.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));
            _logger.Debug($"isNotCompleted: {isNotCompleted}");

            try
            {
                RucaptchaManager.CheckCaptchaWord(message.Value); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу

                if (!isNotCompleted)
                    await ExecuteGet(captchaItem, requestCptchaItems);
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.Error($"Неверно решена каптча. Ответ: {icEx.CaptchaWord}");
                _logger.Warn(ex);
                TryExecuteRequestAgain(captchaItem.RequestId, captchaItem.CurrentActionType, captchaItem.TargetActionType);
                isNotCompleted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Непредвиденная ошибка");
                SendErrorMessage(captchaItem.RequestId, StaticResources.UnexpectedError);
                isNotCompleted = false;
            }

            if (!isNotCompleted)
                RemoveCaptchaCacheItems(captchaItem.RequestId);

            return true;
        }

        private async Task ExecuteGet(CaptchaCacheItem currentCaptchaItem, IEnumerable<CaptchaCacheItem> requestCaptchaItems)
        {
            var request = await _queryExecutor.GetUserRequest(currentCaptchaItem.RequestId);
            _logger.Debug($"Из БД получен Request с ID: {request.Id}.");

            var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == currentCaptchaItem.CurrentActionType);
            _logger.Debug($"Получен обработчик типа: {handler.SupportedActionType}");

            var data = handler.Get(request.RequestObject, requestCaptchaItems);
            _logger.Debug($"Выполнен GET запрос");

            await _queryExecutor.MarkRequestCompleted(currentCaptchaItem.RequestId);

            if (data == null)           //Если нет данных для отправки пользователю
                return;

            _logger.Debug($"Запрос с ID {currentCaptchaItem.RequestId} отмечен выполненным");

            var keyboard = await CreateKeyBoard(request.RequestObject).ConfigureAwait(false);

            foreach (var item in data)
            {
                var msg = new SendToUserMessage(keyboard, request.RequestObject.UserId, item.Key, item.Value);
                _senderActor.Tell(msg, Self);
            }
        }

        #endregion Handlers

        #region Helpers
        private void TryExecuteRequestAgain(int requestId, ActionType currentActionType, ActionType targetActionType)
        {
            _logger.Debug($"Попытка повторного выполнения запроса с ID: {requestId}. CurrentActionType: {currentActionType}. TargetActionType: {targetActionType}");
            //var item = _cacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);

            //if (item == null)
            //    return;

            AddOrUpdateCacheItem(requestId, currentActionType);
            var item = _cacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);

            _logger.Debug($"В кэше повторных запросов найдена запись с ID запроса: {item.RequestId}. CurrentActionType: {item.CurrentActionType}. Попыток: {item.AttemptsCount}. Всего записей: {_cacheItems.Count}");

            if (item.AttemptsCount >= 2 )
            {
                if(currentActionType == ActionType.VinByDiagnosticCard)
                {
                    Self.Tell(new StartActionMessage
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.PolicyNumber,
                        TargetActionType = targetActionType
                    });

                    Self.Tell(new StartActionMessage
                    {
                        RequestId = requestId,
                        CurrentActionType = ActionType.OsagoVechicle,
                        TargetActionType = targetActionType
                    });
                    return;
                }
                SendErrorMessage(requestId, StaticResources.RequestFailedError);
                _logger.Debug($"Из кэша повторных запросов будет удалена запись с ID запроса {item.RequestId}. CurrentActionType {item.CurrentActionType}. AttemptsCount {item.AttemptsCount}. Всего элементов {_cacheItems.Count}");

                _cacheItems.Remove(item);
                _logger.Debug($"В кэше повторных запросов {_cacheItems.Count} элементов");
                return;
            }

            Self.Tell(new StartActionMessage()
            {
                RequestId = requestId,
                CurrentActionType = currentActionType,
                TargetActionType = targetActionType
            });
        }

        private void AddCaptchaCacheItem(int requestId, string captchaId, ActionType currentActionType, ActionType targetActionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CaptchaCacheItem()
            {
                RequestId = requestId,
                CaptchaId = captchaId,
                CurrentActionType = currentActionType,
                TargetActionType = targetActionType,
                SessionId = sessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getPolicyNumberCacheItem);

            _logger.Debug($"В кэш каптчи добавлена запись с RequestId: {requestId}. CaptchaId {captchaId}. currentActionType {currentActionType}. TargetActionType {targetActionType}. SessionId {sessionId}");
        }
        private void RemoveCaptchaCacheItems(int requestId)
        {
            _captchaCacheItems.RemoveAll(x => x.RequestId == requestId);
        }
        private void AddOrUpdateCacheItem(int requestId, ActionType currentActionType)
        {
            var item = _cacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);
            if (item == null)
            {
                _cacheItems.Add(new CacheItem()
                {
                    RequestId = requestId,
                    CurrentActionType = currentActionType
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
            var message = new SendToUserMessage(null, request.RequestObject.UserId, text, null);
            _senderActor.Tell(message, Self);
            _logger.Debug($"Пользователю отправлено сообщение об ошибке");
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(requestTypes, requestObject.GetType());
        }
        #endregion Helpers

    }
}
