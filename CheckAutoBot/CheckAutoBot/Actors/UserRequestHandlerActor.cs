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

        private List<CacheItem> _captchaCacheItems = new List<CacheItem>();

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

            _rsaManager = new RsaManager();
            _gibddManager = new GibddManager();
            _fnpManager = new FnpManager();
            _rucaptchaManager = new RucaptchaManager();
            _eaistoManager = new EaistoManager();
            _random = new Random();
            _actorSelector = new ActorSelector();

            _logger = logger;

            _senderActor = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);


            InitHandlers();

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x =>  CaptchaResponseMessageHadler(x));
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
                var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == message.CurrentActionType);

                if (!CheckCacheItem(message.RequestId, message.CurrentActionType))
                {
                    SendErrorMessage(message.RequestId, StaticResources.RequestFailedError);
                    return;
                }

                var preGetResults = handler.PreGet();
                AddOrUpdateCacheItem(message.RequestId, 
                                     preGetResults.CaptchaId, 
                                     message.CurrentActionType, 
                                     message.TargetActionType, 
                                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                StartActionMessageHandler(message);
                _logger.Warn(ex);
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

            if (captchaItem == null)
                return true;

            captchaItem.CaptchaWord = message.Value;

            var requestCptchaItems = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);
            var isNotCompleted = requestCptchaItems.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));

            if (!isNotCompleted)
            {
                var request = await _queryExecutor.GetUserRequest(captchaItem.RequestId);
                var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == captchaItem.CurrentActionType);

                try
                {
                    var data = handler.Get(request.RequestObject, requestCptchaItems);
                    if (data == null)
                        return true;
                    foreach (var item in data)
                    {
                        var msg = new SendToUserMessage(request.RequestObjectId, request.RequestObject.UserId, item.Key, item.Value);
                        _senderActor.Tell(msg, Self);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (ex is InvalidCaptchaException icEx)
                        _logger.Error("InvalidCaptcha" + icEx);

                    if (captchaItem.AttemptsCount < 1) //(с 0)/если меньше двух
                    {
                        Self.Tell(new StartActionMessage()
                        {
                            RequestId = request.Id,
                            CurrentActionType = captchaItem.CurrentActionType,
                            TargetActionType = captchaItem.TargetActionType
                        });

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Непредвиденная ошибка");
                    SendErrorMessage(request.Id, StaticResources.UnexpectedError);
                }

                _captchaCacheItems.RemoveAll(x => x.RequestId == captchaItem.RequestId);
            }

            return true;
        }

        #endregion Handlers

        #region Helpers
        private bool CheckCacheItem(int requestId, ActionType currentActionType)
        {
            var item = _captchaCacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);
            if (item == null)
                return true;

            return item.AttemptsCount < 1;
        }

        private void AddOrUpdateCacheItem(int requestId, long captchaId, ActionType currentActionType, ActionType targetActionType, string sessionId = null)
        {
            var item = _captchaCacheItems.FirstOrDefault(x => x.RequestId == requestId && x.CurrentActionType == currentActionType);
            if (item == null)
            {
                var getPolicyNumberCacheItem = new CacheItem()
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
            else
            {
                item.CaptchaId = captchaId;
                item.SessionId = sessionId;
                item.AttemptsCount ++;
            }
        }

        private void AddCaptchaRequestToCache(int userRequestId, long captchaId, ActionType currentActionType, ActionType targetActionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CacheItem()
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
