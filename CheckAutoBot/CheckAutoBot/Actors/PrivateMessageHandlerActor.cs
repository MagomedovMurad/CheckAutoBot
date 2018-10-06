using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageHandlerActor: ReceiveActor
    {
        private ActorSelector _actorSelection;
        private readonly ILogger _logger;

        private Regex _regNumberRussianSymbolsRegex;
        private Regex _regNumberLatinSymbolsRegex;
        private Regex _vinCodeRegex;
        private Regex _fioRegex;

        public PrivateMessageHandlerActor(ILogger logger)
        {
            _actorSelection = new ActorSelector();
            _logger = logger;

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
                var userInpuDataTypeWithValue = DefineInputDataType(message.Text);
                //Если сообщение распознано (содержит vin, гос.номер, ФИО)
                if (userInpuDataTypeWithValue.HasValue)
                {
                    var reqestObjectMessage = new UserInputDataMessage()
                    {
                        Data = userInpuDataTypeWithValue.Value.Value,
                        Type = userInpuDataTypeWithValue.Value.Key,
                        UserId = message.FromId,
                        MessageId = message.Id,
                        Date = DateTime.Now
                    };

                    _actorSelection
                        .ActorSelection(Context, ActorsPaths.UserRequestHandlerActor.Path)
                        .Tell(reqestObjectMessage, Self);

                    _logger.Debug($"MessageHandler: define data: {userInpuDataTypeWithValue.Value}");
                }
                else
                {
                    var helpMsg = new HelpMessage() { UserId = message.FromId };
                    _actorSelection
                        .ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path)
                        .Tell(helpMsg, Self);

                    _logger.Debug($"MessageHandler: Send help message");
                }
            }
            //Если сообщение содержит Payload
            else
            {
                var payloadEnvelop = JsonConvert.DeserializeObject<PayloadEnvelop>(message.Payload);
                var payload = JsonConvert.DeserializeObject(payloadEnvelop.Payload, Type.GetType(payloadEnvelop.DotNetType));

                if (payload is RequestPayload requestPayload)
                {
                    var msg = new UserRequestMessage()
                    {
                        MessageId = message.Id,
                        UserId = message.FromId,
                        RequestType = requestPayload.RequestType,
                        Date = DateTime.Now
                    };

                    _actorSelection
                        .ActorSelection(Context, ActorsPaths.UserRequestHandlerActor.Path)
                        .Tell(msg, Self);
                    _logger.Debug($"MessageHandler: Send user request message to UserRequestHandlerActor");
                }
            }
        }

        private KeyValuePair<InputDataType, string>? DefineInputDataType(string inputStr)
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
