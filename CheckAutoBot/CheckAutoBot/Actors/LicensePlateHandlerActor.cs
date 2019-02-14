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
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class LicensePlateHandlerActor: ReceiveActor
    {
        private List<CaptchaCacheItem> _captchaCacheItems;
        private List<CacheItem> _repeatedRequestsCache;
        private DbQueryExecutor _queryExecutor;
        private ILogger _logger;

        private RsaManager _rsaManager;
        private RucaptchaManager _rucaptchaManager;
        private EaistoManager _eaistoManager;
        private KeyboardBuilder _keyboardBuilder;
        private ICanTell _messageSenderActor;


        private IEnumerable<IVinFinderHandler> _handlers;

        public LicensePlateHandlerActor(DbQueryExecutor queryExecutor, ILogger logger)
        {
            _queryExecutor = queryExecutor;
            _logger = logger;

            _keyboardBuilder = new KeyboardBuilder();
            _eaistoManager = new EaistoManager();
            _rucaptchaManager = new RucaptchaManager();
            _rsaManager = new RsaManager();
            _repeatedRequestsCache = new List<CacheItem>();
            _captchaCacheItems = new List<CaptchaCacheItem>();
            _messageSenderActor = new ActorSelector().ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            Receive<StartVinSearchingMessage>(x => StartVinSearch(x));
            Receive<CaptchaResponseMessage>(x => CaptchaResponseMessageHandler(x));

            InitHandlers();
        }

        private void InitHandlers()
        {
            _handlers = new List<IVinFinderHandler>()
            {
                new GetVinByDiagnosticCard(_eaistoManager, _rucaptchaManager),
                new GetPolicyNumberForVinHandler(_rucaptchaManager, _rsaManager),
                new GetOsagoVechicleForVinHandler(_rucaptchaManager, _rsaManager )
            };
        }

        private void StartVinSearch(StartVinSearchingMessage message)
        {
            _logger.Debug($"Запущен поиск вин кода. {Environment.NewLine}" +
                          $"Идентификатор объекта запроса: {message?.RequestObjectId}");
            StartPreGet(message.RequestObjectId, ActionType.DiagnosticCard);
        }

        private async void StartPreGet(int requestObjectId, ActionType actionType)
        {
            try
            {
                _logger.Debug($"Запрос каптчи для {actionType}. Идентификатор объекта запроса: {requestObjectId}.");
                var handler = _handlers.First(x => x.SupportedActionType == actionType);
                var pregetResult = handler.PreGet();
                AddCaptchaCacheItem(requestObjectId, actionType, pregetResult.CaptchaId, pregetResult.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(ex, $"Ошибка при попытке выполнения запроса каптчи. {Environment.NewLine}" +
                                 $"Идентификатор объекта запроса: {requestObjectId}. {Environment.NewLine}");

                var actionTypes = _captchaCacheItems.Where(x => x.Id == requestObjectId).Select(x => x.ActionType);
                TryExecuteRequestAgain(requestObjectId, actionType, actionTypes);
            }
            catch (Exception ex)
            {
                //Очистка кэша
                RemoveCaptchaCacheItems(requestObjectId);
                RemoveRepeatedRequestsCacheItems(requestObjectId);

                //Отправка пользователю сообщения о непредвиденной ошибке
                var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);
                SendMessageToUser(null, requestObject.UserId, StaticResources.UnexpectedError);

                //Отправка сообщения об ошибке мне в вк
                var msg = $"Идентификатор пользователя {requestObject.UserId}. Идентификатор объекта запроса:{requestObjectId}. Ошибка: {ex}";
                SendMessageToUser(null, StaticResources.MyUserId, msg);

                _logger.Error(ex, $"Непредвиденная ошибка при попытке выполнения повторного запроса. {Environment.NewLine}" +
                              $"Идентификатор пользователя {requestObject.UserId}. {Environment.NewLine}" +
                              $"Идентификатор объекта запроса: {requestObjectId}. {Environment.NewLine}");
            }
        }

        private async void CaptchaResponseMessageHandler(CaptchaResponseMessage message)
        {
            _logger.Debug($"Получена каптча с идентификатором: {message.CaptchaId}. Ответ: {message.Value}");

            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return;

            _logger.Debug($"В кэше найдена запись с идентификатором каптчи: {captchaItem.CaptchaId}. {Environment.NewLine}" +
                          $"Для действия: {captchaItem.ActionType}");

            captchaItem.CaptchaWord = message.Value;
            var requestCptchaItems = _captchaCacheItems.Where(x => x.Id == captchaItem.Id);
            var isNotCompleted = requestCptchaItems.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));
            try
            {
                RucaptchaManager.CheckCaptchaWord(message.Value); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу

                if (!isNotCompleted)
                {
                    await ExecuteGet(captchaItem, requestCptchaItems);
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.Warn(icEx, $"Неверно решена каптча. {Environment.NewLine}" +
                                       $"Ответ: {icEx.CaptchaWord}");
                else
                    _logger.Error(ex, $"Идентификатор запроса: {captchaItem.Id}. Тип действия: {captchaItem.ActionType}");

                var actionTypes = requestCptchaItems.Select(x => x.ActionType).ToList();
                TryExecuteRequestAgain(captchaItem.Id, captchaItem.ActionType, actionTypes);
            }
            catch (Exception ex)
            {
                //Очистка кэша
                RemoveCaptchaCacheItems(captchaItem.Id);
                RemoveRepeatedRequestsCacheItems(captchaItem.Id);

                //Отправка пользователю сообщения о непредвиденной ошибке
                var requestObject = await _queryExecutor.GetUserRequestObject(captchaItem.Id);
                SendMessageToUser(null, requestObject.UserId, StaticResources.UnexpectedError);

                //Отправка сообщения об ошибке мне в вк
                var msg = $"Идентификатор пользователя {requestObject.UserId}. {Environment.NewLine}" +
                          $"Идентификатор объекта запроса:{captchaItem?.Id}. {Environment.NewLine}" +
                          $"Ошибка: {ex.StackTrace}";
                SendMessageToUser(null, StaticResources.MyUserId, msg);

                _logger.Error(ex, "Непредвиденная ошибка");
            }
        }

        private async void TryExecuteRequestAgain(int requestObjectId, ActionType actionType, IEnumerable<ActionType> actionTypes)
        {
            RemoveCaptchaCacheItems(requestObjectId);
            bool? eaistoNotAvailable = null;
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestObjectId && x.ActionType == actionType);

            if (item?.AttemptsCount >= 2)
            {
                RemoveRepeatedRequestsCacheItems(requestObjectId);
                if (actionType == ActionType.DiagnosticCard)
                {
                    StartPreGet(requestObjectId, ActionType.PolicyNumber);
                    StartPreGet(requestObjectId, ActionType.OsagoVechicle);
                    eaistoNotAvailable = true;
                }
                else
                {
                    var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);
                    SendMessageToUser(null, requestObject.UserId, StaticResources.RequestFailedError);
                }
            }
            else
            {
                foreach (var type in actionTypes)
                    StartPreGet(requestObjectId, type);
            }

            foreach (var type in actionTypes)
                AddOrUpdateCacheItem(requestObjectId, type, eaistoNotAvailable);
        }

        private async Task ExecuteGet(CaptchaCacheItem cacheItem, IEnumerable<CaptchaCacheItem> requestCaptchaItems)
        {
            var id = cacheItem.Id;
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == id);

            var actionType = cacheItem.ActionType;
            var requestObject = await _queryExecutor.GetUserRequestObject(id) as Auto;

            var handler = _handlers.FirstOrDefault(x => x.SupportedActionType == cacheItem.ActionType);
            var vin = handler.Get(requestObject.LicensePlate, requestCaptchaItems);

            RemoveCaptchaCacheItems(requestObject.Id);
            RemoveRepeatedRequestsCacheItems(requestObject.Id);

            if (!string.IsNullOrWhiteSpace(vin))
            {
                await _queryExecutor.UpdateVinCode(requestObject.Id, vin);

                //Send buttons to user
                var keyboard = await CreateKeyBoard(requestObject);
                var text = $"Гос. номер: {requestObject.LicensePlate}. {Environment.NewLine}" +
                           $"Выберите доступное действие.";
                SendMessageToUser(keyboard, requestObject.UserId, text);

                return;
            }

            if (actionType == ActionType.DiagnosticCard)
            {
                StartPreGet(requestObject.Id, ActionType.PolicyNumber);
                StartPreGet(requestObject.Id, ActionType.OsagoVechicle);
            }
            else
            {

                if (item?.EaistoNotAvailable == true)
                    SendMessageToUser(null, requestObject.UserId, StaticResources.RequestFailedError);
                else
                    SendMessageToUser(null, requestObject.UserId, StaticResources.VinNotFoundError);
            }
        }

        private void AddOrUpdateCacheItem(int requestObjectId, ActionType actionType, bool? eaistoNotAvailable)
        {
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestObjectId && x.ActionType == actionType);
            if (item == null)
            {
                _repeatedRequestsCache.Add(new CacheItem()
                {
                    Id = requestObjectId,
                    ActionType = actionType,
                    EaistoNotAvailable = eaistoNotAvailable,
                    AttemptsCount = 1
                });
            }
            else
            {
                item.AttemptsCount++;
            }
        }

        private void RemoveRepeatedRequestsCacheItems(int requestObjectId)
        {
            _repeatedRequestsCache.RemoveAll(x => x.Id == requestObjectId);
        }

        private void AddCaptchaCacheItem(int id, ActionType actionType, string captchaId, string sessionId)
        {
            _captchaCacheItems.Add(
                new CaptchaCacheItem()
                {
                    Id = id,
                    ActionType = actionType,
                    CaptchaId = captchaId,
                    SessionId = sessionId,
                    Date = DateTime.Now
                });
        }

        private void RemoveCaptchaCacheItems(int requestObjectId)
        {
            _captchaCacheItems.RemoveAll(x => x.Id == requestObjectId);
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            //var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(new List<RequestType>(), requestObject.GetType());
        }

        private void SendMessageToUser(Keyboard keyboard, int userId, string text)
        {
            var msg = new SendToUserMessage(userId, text, keyboard: keyboard); 
            _messageSenderActor.Tell(msg, Self);
        }
    }
}
