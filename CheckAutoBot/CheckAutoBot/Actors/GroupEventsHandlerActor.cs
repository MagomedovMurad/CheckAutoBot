using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.GroupModels;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class GroupEventsHandlerActor : ReceiveActor
    {
        private ICanSelectActor _actorSelector;
        private readonly ICustomLogger _logger;

        public GroupEventsHandlerActor(ICustomLogger logger)
        {
            _actorSelector = new ActorSelector();
            _logger = logger;

            Receive<string>(x => GroupEventMessageHandler(x));
        }

        private void GroupEventMessageHandler(string json)
        {
            var message = JsonConvert.DeserializeObject<GroupEventMessage>(json);

            switch (message.EventType)
            {
                case Enums.VkEventType.NewMessage:
                    var privateMessage = message.JsonObject.ToObject<PrivateMessage>();
                    PrivateMessageHandler(privateMessage); 
                    break;
                case Enums.VkEventType.AllowMessage:
                    var messagesAllowedEvent = message.JsonObject.ToObject<MessagesAllowedEvent>();
                    MessageAllowEventHandler(messagesAllowedEvent);
                    break;
                case Enums.VkEventType.DenyMessage:
                    var userId = message.JsonObject.ToObject<int>();
                    MessageDenyEventHandler(userId);
                    break;
                default:
                    _logger.WriteToLog(LogLevel.Error, $"Не найден обработчик для типа «{message.EventType}»", true);
                    break;
            }
        }

        private void PrivateMessageHandler(PrivateMessage message)
        {
            _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageHandlerActor.Path).Tell(message, Self);
            var debug = $"PrivateMessageHandler. Send message to PrivateMessageHandlerActor. PrivateMessage.Text={message.Text}";
            _logger.WriteToLog(LogLevel.Debug, debug);
        }

        private void MessageAllowEventHandler(MessagesAllowedEvent @event)
        {
            var message = new MessagesAllowedEventMessage(@event.UserId);
            _actorSelector.ActorSelection(Context, ActorsPaths.SubscribersActionsHandlerActor.Path).Tell(message, Self);
            var debug = $"Пользователь с идентификатором {@event.UserId} разрешил отправку личных сообщений";
            _logger.WriteToLog(LogLevel.Debug, debug, true);
        }

        private void MessageDenyEventHandler(int userId)
        {
            var warn = $"Пользователь с идентификатором {userId} запретил отправку личных сообщений";
            _logger.WriteToLog(LogLevel.Warn, warn, true);
        }


    }
}
