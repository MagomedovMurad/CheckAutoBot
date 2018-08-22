using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class GroupEventsHandlerActor : ReceiveActor
    {
        public GroupEventsHandlerActor()
        {
            Receive<GroupEventMessage>(x => GroupEventMessageHandler(x));
        }

        private void GroupEventMessageHandler(GroupEventMessage message)
        {
            switch (message.EventType)
            {
                case Enums.VkEventType.NewMessage:
                    var privateMessage = JsonConvert.DeserializeObject<PrivateMessage>(message.JsonObject);
                    
                    break;
            }
        }
    }
}
