using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class GroupEventsHandlerActor : ReceiveActor
    {
        private ICanSelectActor _actorSelector;

        public GroupEventsHandlerActor()
        {
            _actorSelector = new ActorSelector();
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
                default:
                    throw new InvalidOperationException($"Не найден обработчик для типа \"{message.EventType}\"");
            }
        }

        private void PrivateMessageHandler(PrivateMessage message)
        {
            _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageHandlerActor.Path).Tell(message, Self);
        }
    }
}
