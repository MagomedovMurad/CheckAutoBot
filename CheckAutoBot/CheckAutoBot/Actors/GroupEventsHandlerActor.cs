using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
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
        private readonly ILogger _logger;

        public GroupEventsHandlerActor(ILogger logger)
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
                    _logger.Error($"Не найден обработчик для типа \"{message.EventType}\"");
                    break;
            }
        }

        private void PrivateMessageHandler(PrivateMessage message)
        {
            _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageHandlerActor.Path).Tell(message, Self);
            _logger.Debug($"PrivateMessageHandler. Send message to PrivateMessageHandlerActor. PrivateMessage.Text={message.Text}");
        }

        private void MessageAllowEventHandler(MessagesAllowedEvent messagesAllowedEvent)
        {

        }

        private void MessageDenyEventHandler(int userId)
        {

        }


    }
}
