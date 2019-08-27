using Akka.Actor;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Handlers;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class VinCodeHandlerActor : ReceiveActor
    {
        private readonly ICustomLogger _logger;
        private readonly HistoryHandler _historyHandler;
        ICanTell _senderActor;
        private ICanSelectActor _actorSelector;
        private DbQueryExecutor _queryExecutor;
        private KeyboardBuilder _keyboardBuilder;
        IActorRef _self;

        private readonly Guid _subcriberId = Guid.Parse("41710BB8-809B-4259-B940-CAF3D02B5970");
        //private List<CaptchaCacheItem> _captchaCacheItems = new List<CaptchaCacheItem>();
        private List<CacheItem> _repeatedRequestsCache = new List<CacheItem>();

        private CaptchaCacheManager _captchaCacheManager;

        public VinCodeHandlerActor(ICustomLogger logger, DbQueryExecutor queryExecutor, CaptchaCacheManager captchaCacheManager)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _historyHandler = new HistoryHandler(new GibddManager(), new RucaptchaManager());
            _actorSelector = new ActorSelector();
            _senderActor = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);
            _keyboardBuilder = new KeyboardBuilder();
            _self = Self;
            _captchaCacheManager = captchaCacheManager;

            _captchaCacheManager.Subscribe(_subcriberId, CaptchaResponseMessageHandler);

            ReceiveAsync<StartGeneralInfoSearchMessage>(x => GetGeneralInfo(x));
            //ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
        }

        private async Task GetGeneralInfo(StartGeneralInfoSearchMessage message)
        {
            try
            {
                _logger.WriteToLog(LogLevel.Debug, $"Запрос каптчи для {ActionType.History}. Идентификатор объекта запроса: {message.RequestObjectId}.");

                var preGetResults = _historyHandler.PreGet();

                _captchaCacheManager.AddToCaptchaCache(message.RequestObjectId,
                                     ActionType.History,
                                     preGetResults.CaptchaId,
                                     preGetResults.SessionId,
                                     _subcriberId);

                //AddCaptchaCacheItem(message.RequestObjectId,
                //                     preGetResults.CaptchaId,
                //                     ActionType.History,
                //                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.WriteToLog(LogLevel.Warn, $"Ошибка при попытке выполнения запроса каптчи. {Environment.NewLine}" +
                                $"Идентификатор объекта запроса: {message.RequestObjectId}. {ex}", true);

                await TryExecuteRequestAgain(message.RequestObjectId);
            }
            catch (Exception ex)
            {
                //Очистка кэша
                //RemoveCaptchaCacheItems(message.RequestObjectId);
                RemoveRepeatedRequestsCacheItems(message.RequestObjectId);
                //await _queryExecutor.ChangeRequestStatus(message.RequestId, false);

                //Отправка пользователю сообщения о непредвиденной ошибке
                SendErrorMessage(message.RequestObjectId, StaticResources.UnexpectedError);

                var msg = $"Идентификатор объекта запроса:{message.RequestObjectId}. {Environment.NewLine}" +
                          $"Ошибка: {ex}";
                _logger.WriteToLog(LogLevel.Error, msg, true);
            }
        }

        private async Task CaptchaResponseMessageHandler(IEnumerable<ActionCacheItem> items)
        {
            //var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            var firstCaptchaItem = items.First();

            //if (captchaItem == null)
            //    return true;

            //captchaItem.CaptchaWord = message.Value;
            int requestId = firstCaptchaItem.Id;

            try
            {
                RucaptchaManager.CheckCaptchaWord(firstCaptchaItem.CaptchaWord); // Выбросит исключение, если от Rucaptcha вернулась ошибка вместо ответа на каптчу
                await ExecuteGet(firstCaptchaItem);

                //RemoveCaptchaCacheItems(requestId);
                RemoveRepeatedRequestsCacheItems(requestId);
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                    _logger.WriteToLog(LogLevel.Warn, $"Неверно решена каптча. {Environment.NewLine}" +
                                       $"Ответ: {icEx.CaptchaWord}");
                else
                    _logger.WriteToLog(LogLevel.Error, $"Идентификатор запроса: {firstCaptchaItem.Id}. Тип действия: {firstCaptchaItem.ActionType}. {ex}", true);

                await TryExecuteRequestAgain(firstCaptchaItem.Id);
            }
            catch (Exception ex)
            {
                //RemoveCaptchaCacheItems(requestId);
                RemoveRepeatedRequestsCacheItems(requestId);
                // await _queryExecutor.ChangeRequestStatus(requestId, false);

                var msg = $"Идентификатор запроса:{requestId}. {Environment.NewLine}" +
                          $"Ошибка: {ex}";
                _logger.WriteToLog(LogLevel.Error, msg, true);
                SendErrorMessage(firstCaptchaItem.Id, StaticResources.UnexpectedError);
            }

            return;
        }

        private async Task ExecuteGet(ActionCacheItem сaptchaItem)
        {
            var requestObject = await _queryExecutor.GetUserRequestObject(сaptchaItem.Id);
            var result = _historyHandler.Get(requestObject, сaptchaItem.CaptchaWord, сaptchaItem.SessionId);

            var auto = requestObject as Auto;
            var autoData = auto.LicensePlate ?? auto.Vin;
            string data;
            if (result == null)
            {
                data = auto.LicensePlate != null ? $"гос. номеру {autoData}" : $"VIN коду {autoData}";
                SendErrorMessage(requestObject.Id, $"😕 К сожалению не удалось найти информацию по {data}");
                return;
            }

            var resultJson = JsonConvert.SerializeObject(result);
            var objectCache = new RequestObjectCache()
            {
                RequestObjectId = requestObject.Id,
                Data = resultJson
            };

            await _queryExecutor.AddRequestObjectCacheItem(objectCache);

            var keyboard = await CreateKeyBoard(requestObject).ConfigureAwait(false);
            data = auto.LicensePlate != null ? $"Гос. номер: {autoData}" : $"VIN код: {autoData}";
            var text = $"✏ {data}{Environment.NewLine}" +
                       $"🚗 {result.Vehicle.Model}, {result.Vehicle.Year}г.{Environment.NewLine}" +
                       $"⬇ Выберите доступное действие.";

            var msg = new SendToUserMessage(requestObject.UserId, text, keyboard: keyboard);
            _senderActor.Tell(msg, _self);
        }

        private async Task TryExecuteRequestAgain(int requestObjectId)
        {
           // _logger.Debug($"Попытка повторного выполнения запроса с requestObjectID: {requestObjectId}. CurrentActionType: {ActionType.History}");

            AddOrUpdateCacheItem(requestObjectId, ActionType.History);
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestObjectId && x.ActionType == ActionType.History);

            //_logger.Debug($"В кэше повторных запросов найдена запись с ID запроса: {item.Id}. CurrentActionType: {item.ActionType}. Попыток: {item.AttemptsCount}. Всего записей: {_repeatedRequestsCache.Count}");

            if (item.AttemptsCount >= 2)
            {
                SendErrorMessage(requestObjectId, StaticResources.RequestFailedError);
              //  _logger.Debug($"Из кэша повторных запросов будет удалена запись с ID запроса {item.Id}. CurrentActionType {item.ActionType}. AttemptsCount {item.AttemptsCount}. Всего элементов {_repeatedRequestsCache.Count}");

                _repeatedRequestsCache.Remove(item);
              //  _logger.Debug($"В кэше повторных запросов {_repeatedRequestsCache.Count} элементов");
                return;
            }

            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            var msg = new StartGeneralInfoSearchMessage()
            {
                Vin = (requestObject as Auto).Vin,
                RequestObjectId = requestObjectId
            };

            await GetGeneralInfo(msg);
        }

        //private void AddCaptchaCacheItem(int requestId, string captchaId, ActionType currentActionType, string sessionId = null)
        //{
        //    var getPolicyNumberCacheItem = new CaptchaCacheItem()
        //    {
        //        Id = requestId,
        //        CaptchaId = captchaId,
        //        ActionType = currentActionType,
        //        SessionId = sessionId,
        //        Date = DateTime.Now
        //    };
        //    _captchaCacheItems.Add(getPolicyNumberCacheItem);

        //    _logger.Debug($"В кэш каптчи добавлена запись с RequestId: {requestId}. CaptchaId {captchaId}. currentActionType {currentActionType}. SessionId {sessionId}");
        //}

        //private void RemoveCaptchaCacheItems(int requestId)
        //{
        //    _captchaCacheItems.RemoveAll(x => x.Id == requestId);
        //}

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
               // _logger.Debug($"В кэш повторных запросов добавлена запись с RequestId {requestId}. CurrentActionType {currentActionType}");
            }
            else
            {
                item.AttemptsCount++;
               // _logger.Debug($"В кэше повторных запросов обновлена запись с RequestId {requestId}. CurrentActionType {currentActionType}. AttemptsCount: {item.AttemptsCount}");
            }
        }

        private void RemoveRepeatedRequestsCacheItems(int requestObjectId)
        {
            _repeatedRequestsCache.RemoveAll(x => x.Id == requestObjectId);
        }

        private async void SendErrorMessage(int requestObjectId, string text)
        {
            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            var message = new SendToUserMessage(requestObject.UserId, text);
            _senderActor.Tell(message, _self);
        }

        private void SendErrorMe(string text)
        {
            var message = new SendToUserMessage(StaticResources.MyUserId, text);
            _senderActor.Tell(message, Self);
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            //var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(new List<RequestType>(), requestObject.GetType());
        }
    }
}
