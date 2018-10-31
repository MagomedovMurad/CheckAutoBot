using Akka.Actor;
using CheckAutoBot.EaistoModels;
using CheckAutoBot.Enums;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Handlers;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.PledgeModels;
using CheckAutoBot.RsaModels;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

            _senderActor = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            InitHandlers();

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
            Receive<StartActionMessage>(x => StartActionMessageHandler(x));
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
        private void StartActionMessageHandler(StartActionMessage message)
        {
            try
            {
                AddOrUpdateCacheItem(message.RequestId, message.CurrentActionType);
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
                var msg = new SendToUserMessage(null, message.UserId, text, null);

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
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);
            try
            {
                RucaptchaManager.CheckCaptchaWord(message.Value); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу

                if (captchaItem == null)
                    throw new Exception($"В кэше не найдена запись с идентификатором каптчи {message.CaptchaId}");

                captchaItem.CaptchaWord = message.Value;

                var requestCptchaItems = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);
                var isNotCompleted = requestCptchaItems.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));

                if (!isNotCompleted)
                    await ExecuteGet(captchaItem, requestCptchaItems);
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.Error("InvalidCaptcha" + icEx);
                _logger.Warn(ex);
                TryExecuteRequestAgain(captchaItem.RequestId, captchaItem.CurrentActionType, captchaItem.TargetActionType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Непредвиденная ошибка");
                SendErrorMessage(captchaItem.RequestId, StaticResources.UnexpectedError);
                _cacheItems.RemoveAll(x => x.RequestId == captchaItem.RequestId && x.CurrentActionType == captchaItem.CurrentActionType);
            }

            _captchaCacheItems.RemoveAll(x => x.RequestId == captchaItem.RequestId);

            return true;
        }

        private async Task ExecuteGet(CaptchaCacheItem currentCaptchaItem, IEnumerable<CaptchaCacheItem> requestCaptchaItems)
        {
            var request = await _queryExecutor.GetUserRequest(currentCaptchaItem.RequestId);
            var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == currentCaptchaItem.CurrentActionType);

            var data = handler.Get(request.RequestObject, requestCaptchaItems);

            await _queryExecutor.MarkRequestCompleted(currentCaptchaItem.RequestId);

            if (data == null)           //Если нет данных для отправки пользователю
                return;

            foreach (var item in data)
            {
                var msg = new SendToUserMessage(request.RequestObjectId, request.RequestObject.UserId, item.Key, item.Value);
                _senderActor.Tell(msg, Self);
            }
        }

        #endregion Handlers

        #region Helpers
        private void TryExecuteRequestAgain(int requestId, ActionType currentActionType, ActionType targetActionType)
        {
            var item = _cacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);

            if (item == null)
                return;

            if (item.AttemptsCount == 1)
            {
                if(currentActionType == ActionType.VinByDiagnosticCard)
                {
                    Self.Tell(new StartActionMessage
                    {
                        RequestId = requestId,
                        CurrentActionType = currentActionType,
                        TargetActionType = targetActionType
                    });
                    return;
                }
                SendErrorMessage(requestId, StaticResources.RequestFailedError);
                _cacheItems.Remove(item);

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
            }
            else
            {
                item.AttemptsCount++;
            }
        }

        private void AddCaptchaRequestToCache(int userRequestId, string captchaId, ActionType currentActionType, ActionType targetActionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CaptchaCacheItem()
            {
                RequestId = userRequestId,
                CaptchaId = captchaId,
                CurrentActionType = currentActionType,
                TargetActionType = targetActionType,
                SessionId = sessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getPolicyNumberCacheItem);
        }

        private async void SendErrorMessage(int requestId, string text)
        {
            var request = await _queryExecutor.GetUserRequest(requestId);
            var message = new SendToUserMessage(request.RequestObjectId, request.RequestObject.UserId, text, null);
            _senderActor.Tell(message, Self);
        }
        #endregion Helpers

    }
}
