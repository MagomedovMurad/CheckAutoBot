using CheckAutoBot.Infrastructure.Messages;
using CheckAutoBot.Messages;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.Enums;
using CheckAutoBot.Vk.Api.GroupModels;
using CheckAutoBot.Vk.Api.MessagesModels;
using EasyNetQ;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Controllers
{
    public interface IGroupEventsController
    {
        Task HandleGroupEvent(string json);
    }

    public class GroupEventsController: IGroupEventsController
    {
        private readonly ICustomLogger _logger;
        private readonly IBus _bus;
        private IPrivateMessagesController _privateMessagesController;

        public GroupEventsController(ICustomLogger logger, IBus bus, IPrivateMessagesController privateMessagesController)
        {
            _logger = logger;
            _bus = bus;
            _privateMessagesController = privateMessagesController;
        }

        public async Task HandleGroupEvent(string json)
        {
            var message = JsonConvert.DeserializeObject<GroupEventEnvelop>(json);

            switch (message.EventType)
            {
                case EventType.NewMessage:
                    var privateMessage = message.JsonObject.ToObject<PrivateMessage>();
                    await PrivateMessageHandler(privateMessage); 
                    break;
                case EventType.AllowMessage:
                    var messagesAllowedEvent = message.JsonObject.ToObject<MessagesAllowedEvent>();
                    await MessageAllowEventHandler(messagesAllowedEvent);
                    break;
                case EventType.DenyMessage:
                    var userId = message.JsonObject.ToObject<int>();
                    await MessageDenyEventHandler(userId);
                    break;
                default:
                    _logger.WriteToLog(LogLevel.Error, $"Не найден обработчик для типа «{message.EventType}»", true);
                    break;
            }
        }

        private async Task PrivateMessageHandler(PrivateMessage privateMessage)
        {
            _privateMessagesController.HandleMessage(privateMessage);
            _logger.WriteToLog(LogLevel.Debug, $"Новое сообщение ВКонтакте: {privateMessage.Text}");
        }

        private async Task MessageAllowEventHandler(MessagesAllowedEvent messagesAllowedEvent)
        {
            var message = new MessagesAllowedEventMessage(messagesAllowedEvent.UserId);
            await _bus.PublishAsync(message);
            var debug = $"Пользователь с идентификатором {messagesAllowedEvent.UserId} разрешил отправку личных сообщений";
            _logger.WriteToLog(LogLevel.Debug, debug, true);
        }

        private async Task MessageDenyEventHandler(int userId)
        {
            var warn = $"Пользователь с идентификатором {userId} запретил отправку личных сообщений";
            _logger.WriteToLog(LogLevel.Warn, warn, true);
        }
    }
}
