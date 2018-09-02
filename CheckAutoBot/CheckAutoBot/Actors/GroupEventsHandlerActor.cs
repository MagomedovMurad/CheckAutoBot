using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Models;
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
            var regNumberRussianSymbols = @"[АВЕКМНОРСТУХавекмнорстух][\d]{3}[АВЕКМНОРСТУХавекмнорстух]{2}[\d]{2,3}";
            var regNumberLatinSymbols = @"[ABEKMHOPCTYXabekmhopctyx][\d]{3}[ABEKMHOPCTYXabekmhopctyx]{2}[\d]{2,3}";

            var vinCode = "[0123456789ABCDEFGHJKLMNPRSTUVWXYZabcdefghjklmnprstuvwxyz]{17}";
            var fio =  @"([\s]?[А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s]?[А-ЯЁа-яё]+)";



            if (message.)
            {

            }





            new UserRequest()
            {
                Id = Guid.NewGuid(),
                Date = DateTimeOffset.UtcNow,
                MessageId = message.Id,
                InputData = message.Text,
                Type = Enums.UserRequestType.Dtp,
                UserId = message.FromId
            };
        }
    }
}
