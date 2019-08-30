using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Messages;
using CheckAutoBot.Managers;
using EasyNetQ;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CheckAutoBot.Utils
{
    public class CaptchaCacheManager
    {
        private readonly IBus _bus;
        private readonly ICustomLogger _logger;
        private RucaptchaManager _rucaptchaManager;

        private List<CaptchaCacheItem> _captchaCacheItems;
        private Dictionary<Guid, Func<IEnumerable<ActionCacheItem>, Task>> _subscribersCache;

        Timer _timer;

        public CaptchaCacheManager(IBus bus, ICustomLogger logger)
        {
            _bus = bus;
            _logger = logger;
            _rucaptchaManager = new RucaptchaManager();
            _bus.SubscribeAsync<CaptchaResponseMessage>("CaptchaManager", CaptchaResponseMessageHandler);
            _captchaCacheItems = new List<CaptchaCacheItem>();
            _subscribersCache = new Dictionary<Guid, Func<IEnumerable<ActionCacheItem>, Task>>();

            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var itemsWitoutAnswer = _captchaCacheItems.Where(x => (DateTime.Now - x.Date).TotalSeconds > 180).ToArray();
            if (!itemsWitoutAnswer.Any())
                return;

            foreach (var item in itemsWitoutAnswer)
            {
                var captchaWord = _rucaptchaManager.GetCaptchaResult(item.CaptchaId);
                var message = new CaptchaResponseMessage(item.CaptchaId, captchaWord);
                CaptchaResponseMessageHandler(message);
            }
        }

        private async Task CaptchaResponseMessageHandler(CaptchaResponseMessage message)
        {
            try
            {
                _logger.WriteToLog(LogLevel.Debug, $"Получена каптча с идентификатором: {message.CaptchaId}");

                var item = _captchaCacheItems.Single(x => x.CaptchaId == message.CaptchaId);

                if (item is null)
                {
                    var error = $"В кэше не найдена запись с идентификатором каптчи {message.CaptchaId}";
                    _logger.WriteToLog(LogLevel.Error, error, true);
                    return;
                }

                var debug = $"В кэше найдена запись с идентификатором каптчи: {item.CaptchaId}." +
                              $"Для действия: {item.ActionType}" +
                              $"ID: {item.Id}";
                _logger.WriteToLog(LogLevel.Debug, debug, false);

                item.CaptchaWord = message.Value;

                var items = _captchaCacheItems.Where(x => x.Id == item.Id);
                var isNotCompleted = items.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));

                if (isNotCompleted)
                {
                    var debugMsg = $"Ожидание следующей каптчи для запроса с ID: {item.Id}";
                    _logger.WriteToLog(LogLevel.Debug, debugMsg, false);
                    return;
                }

                var action = _subscribersCache[item.SubscriberId];
                var actionItems = ConvertCaptchaToCacheItems(items.ToArray());
                _captchaCacheItems.RemoveAll(x => x.Id == item.Id);
                await action.Invoke(actionItems);
            }
            catch (Exception ex)
            {
                _logger.WriteToLog(LogLevel.Error, $"Ошибка при обработке полученной каптчи: {ex}", true);
            }
        }

        private IEnumerable<ActionCacheItem> ConvertCaptchaToCacheItems(IEnumerable<CaptchaCacheItem> items)
        {
            return items.Select(x => new ActionCacheItem()
            {
                Id = x.Id,
                ActionType = x.ActionType,
                CaptchaId = x.CaptchaId,
                CaptchaWord = x.CaptchaWord,
                SessionId = x.SessionId
            });
        }

        public void Subscribe(Guid subscriberId, Func<IEnumerable<ActionCacheItem>, Task> action)
        {
            _subscribersCache[subscriberId] = action;
        }

        public void Unsubscribe(Guid subscriberId)
        {
            _subscribersCache.Remove(subscriberId);
        }

        public void AddToCaptchaCache(int id, ActionType actionType, string captchaId, string sessionId, Guid subscriberId)
        {
            var item = new CaptchaCacheItem()
            {
                Id = id,
                CaptchaId = captchaId,
                ActionType = actionType,
                SessionId = sessionId,
                Date = DateTime.Now,
                CaptchaWord = null,
                SubscriberId = subscriberId
            };
            _captchaCacheItems.Add(item);
        }
    }
}
