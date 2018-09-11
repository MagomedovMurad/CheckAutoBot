using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageHandlerActor: ReceiveActor
    {
        ActorSelector _actorSelection;

        Regex _regNumberRussianSymbolsRegex;
        Regex _regNumberLatinSymbolsRegex;
        Regex _vinCodeRegex;
        Regex _fioRegex;

        public PrivateMessageHandlerActor()
        {
            _actorSelection = new ActorSelector();

            Receive<PrivateMessage>(x => MessageHandler(x));
        }

        protected override void PreStart()
        {
            var regNumberRussianSymbolsPattern = @"[АВЕКМНОРСТУХавекмнорстух][\d]{3}[АВЕКМНОРСТУХавекмнорстух]{2}[\d]{2,3}";
            var regNumberLatinSymbolsPattern = @"[ABEKMHOPCTYXabekmhopctyx][\d]{3}[ABEKMHOPCTYXabekmhopctyx]{2}[\d]{2,3}";
            var vinCodePattern = "[0123456789ABCDEFGHJKLMNPRSTUVWXYZabcdefghjklmnprstuvwxyz]{17}";
            var fioPattern = @"([\s]?[А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s]?[А-ЯЁа-яё]+)";

            _regNumberRussianSymbolsRegex = new Regex(regNumberRussianSymbolsPattern);
            _regNumberLatinSymbolsRegex = new Regex(regNumberLatinSymbolsPattern);
            _vinCodeRegex = new Regex(vinCodePattern);
            _fioRegex = new Regex(fioPattern);
        }

        public void MessageHandler(PrivateMessage message)
        {
            //Если сообщение НЕ содержит Payload. (Значит это данные об объекте(vin, гос.номер, ФИО))
            if (message.Payload == null)
            {
                var requestObjectTypeWithValue = DefineRequestObjectType(message.Text);
                //Если сообщение распознано (содержит vin, гос.номер, ФИО)
                if (requestObjectTypeWithValue.HasValue)
                {
                    var reqestObjectMessage = new UserRequestObjectMessage()
                    {
                        Data = requestObjectTypeWithValue.Value.Value,
                        Type = requestObjectTypeWithValue.Value.Key,
                        UserId = message.FromId,
                        Date = DateTime.Now
                    };

                    _actorSelection
                        .ActorSelection(Context, ActorsPaths.UserRequestHandlerActor.Path)
                        .Tell(reqestObjectMessage, Self);
                }
                else
                {
                    var helpMsg = new HelpMessage() { UserId = message.FromId };
                    _actorSelection
                        .ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path)
                        .Tell(helpMsg, Self);
                }
            }
            //Если сообщение содержит Payload. (Значит это запрос данных по объекту)
            else
            { 
                var type = DefineRequestType(message.Payload);

                var requestMessage = new UserRequestMessage()
                {
                    UserId = message.FromId,
                    MessageId = message.Id,
                    RequestType = type
                };

                _actorSelection
                        .ActorSelection(Context, ActorsPaths.UserRequestHandlerActor.Path)
                        .Tell(requestMessage, Self);
            }
        }

        private KeyValuePair<InputDataType, string>? DefineRequestObjectType(string inputStr)
        {
            Match match;
            match = _regNumberRussianSymbolsRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<InputDataType, string>(InputDataType.LicensePlate, match.Value);

            match = _regNumberLatinSymbolsRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<InputDataType, string>(InputDataType.LicensePlate, match.Value);

            match = _vinCodeRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<InputDataType, string>(InputDataType.Vin, match.Value);

            match = _fioRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<InputDataType, string>(InputDataType.FullName, match.Value);

            return null;
        }

        private RequestType DefineRequestType(string stringPayload)
        {
            var payload = JsonConvert.DeserializeObject<RequestPayload>(stringPayload);
            return payload.RequestType;
        }
    }
}
