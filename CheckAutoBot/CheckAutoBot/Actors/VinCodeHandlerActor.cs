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
    public class VinCodeHandlerActor : ReceiveActor
    {
        private readonly ILogger _logger;
        private readonly IHandler _historyHandler;
        ICanTell _senderActor;
        private ICanSelectActor _actorSelector;
        private DbQueryExecutor _queryExecutor;
        private KeyboardBuilder _keyboardBuilder;

        private List<CaptchaCacheItem> _captchaCacheItems = new List<CaptchaCacheItem>();
        private List<CacheItem> _repeatedRequestsCache = new List<CacheItem>();

        public VinCodeHandlerActor(ILogger logger, DbQueryExecutor queryExecutor)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _historyHandler = new HistoryHandler(new GibddManager(), new RucaptchaManager());
            _actorSelector = new ActorSelector();
            _senderActor = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);
            _keyboardBuilder = new KeyboardBuilder();

            ReceiveAsync<StartGeneralInfoSearchMessage>(x => GetGeneralInfo(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
        }

        private async Task GetGeneralInfo(StartGeneralInfoSearchMessage message)
        {
            try
            {
                _logger.Debug($"Запрос каптчи для {ActionType.History}. Идентификатор объекта запроса: {message.RequestObjectId}.");

                var preGetResults = _historyHandler.PreGet();
                AddCaptchaCacheItem(message.RequestObjectId,
                                     preGetResults.CaptchaId,
                                     ActionType.History,
                                     preGetResults.SessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(ex, $"Ошибка при попытке выполнения запроса каптчи. {Environment.NewLine}" +
                                $"Идентификатор объекта запроса: {message.RequestObjectId}. {Environment.NewLine}");

                await TryExecuteRequestAgain(message.RequestObjectId);
            }
            catch (Exception ex)
            {
                //Очистка кэша
                RemoveCaptchaCacheItems(message.RequestObjectId);
                RemoveRepeatedRequestsCacheItems(message.RequestObjectId);
                //await _queryExecutor.ChangeRequestStatus(message.RequestId, false);

                //Отправка пользователю сообщения о непредвиденной ошибке
                SendErrorMessage(message.RequestObjectId, StaticResources.UnexpectedError);
                _logger.Error(ex, "Непредвиденная ошибка");

                //Отправка сообщения об ошибке мне в вк
                var msg = $"Идентификатор объекта запроса:{message.RequestObjectId}. {Environment.NewLine}" +
                          $"Ошибка: {ex}";
                SendErrorMe(msg);
            }
        }

        private async Task<bool> CaptchaResponseMessageHadler(CaptchaResponseMessage message)
        {
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return true;

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
                await TryExecuteRequestAgain(captchaItem.Id);
            }
            catch (Exception ex)
            {
                RemoveCaptchaCacheItems(requestId);
                RemoveRepeatedRequestsCacheItems(requestId);
               // await _queryExecutor.ChangeRequestStatus(requestId, false);

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
            var requestObject = await _queryExecutor.GetUserRequestObject(сaptchaItem.Id);
            var data = _historyHandler.Get(requestObject, сaptchaItem);

            if (data == null)
            {
                //К сожалению не удалось найти информацию по гос.номеру/вин коду
            }

            var keyboard = await CreateKeyBoard(requestObject).ConfigureAwait(false);
            foreach (var item in data)
            {
                var msg = new SendToUserMessage(requestObject.UserId, item.Key, item.Value, keyboard);
                _senderActor.Tell(msg, Self);
            }
        }

        private async Task TryExecuteRequestAgain(int requestObjectId)
        {
            _logger.Debug($"Попытка повторного выполнения запроса с requestObjectID: {requestObjectId}. CurrentActionType: {ActionType.History}");

            AddOrUpdateCacheItem(requestObjectId, ActionType.History);
            var item = _repeatedRequestsCache.FirstOrDefault(x => x.Id == requestObjectId && x.ActionType == ActionType.History);

            _logger.Debug($"В кэше повторных запросов найдена запись с ID запроса: {item.Id}. CurrentActionType: {item.ActionType}. Попыток: {item.AttemptsCount}. Всего записей: {_repeatedRequestsCache.Count}");

            if (item.AttemptsCount >= 2)
            {
                SendErrorMessage(requestObjectId, StaticResources.RequestFailedError);
                _logger.Debug($"Из кэша повторных запросов будет удалена запись с ID запроса {item.Id}. CurrentActionType {item.ActionType}. AttemptsCount {item.AttemptsCount}. Всего элементов {_repeatedRequestsCache.Count}");

                _repeatedRequestsCache.Remove(item);
                _logger.Debug($"В кэше повторных запросов {_repeatedRequestsCache.Count} элементов");
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

        private async void SendErrorMessage(int requestObjectId, string text)
        {
            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            var message = new SendToUserMessage(requestObject.UserId, text);
            _senderActor.Tell(message, Self);
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
