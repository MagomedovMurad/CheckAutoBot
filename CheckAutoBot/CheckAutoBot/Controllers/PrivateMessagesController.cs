using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CheckAutoBot.Controllers
{
    public class PrivateMessagesController
    {
        private readonly ICustomLogger _logger;
        private DbQueryExecutor _queryExecutor;
        private MessagesSenderController _messagesSenderController;
        private InputDataController _inputDataController;

        private Regex _regNumberRegex;
        private Regex _vinCodeRegex;
        //private Regex _fioRegex;

        public PrivateMessagesController(DbQueryExecutor queryExecutor, ICustomLogger logger)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
        }

        private Dictionary<string, string> _latinToCyrillicSymbols = new Dictionary<string, string>()
        {
            { "A", "А"},
            { "B", "В" },
            { "E", "Е"},
            { "K", "К"},
            { "M", "М"},
            { "H", "Н"},
            { "O", "О"},
            { "P", "Р"},
            { "C", "С"},
            { "T", "Т"},
            { "Y", "У"},
            { "X", "Х"}
        };

        private void Initialize()
        {
            var regNumberPattern = @"[ABEKMHOPCTYXabekmhopctyxАВЕКМНОРСТУХавекмнорстух][\d]{3}[ABEKMHOPCTYXabekmhopctyxАВЕКМНОРСТУХавекмнорстух]{2}[\d]{2,3}";
            var vinCodePattern = @"[0123456789ABCDEFGHJKLMNPRSTUVWXYZabcdefghjklmnprstuvwxyz]{17}";
            //var fioPattern = @"([\s]?[А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s][А-ЯЁа-яё\-]+[\s]?[А-ЯЁа-яё]+)";

            _regNumberRegex = new Regex(regNumberPattern);
            _vinCodeRegex = new Regex(vinCodePattern);
            //_fioRegex = new Regex(fioPattern);
        }

        public void HandleMessage(PrivateMessage message)
        {
            //Если сообщение НЕ содержит Payload. (Значит это данные об объекте(vin, гос.номер, ФИО))
            if (message.Payload == null)
                UserRequestObjectHandler(message);

            //Если сообщение содержит Payload
            else
                UserRequestHandler(message);
        }

        private void UserRequestObjectHandler(PrivateMessage message)
        {
            var inputData = TryPullInputDataFromMessage(message.Text);
            if (inputData is null)
            {
                //Если сообщение не распознано (не содержит vin, гос.номер, ФИО)
                SendHelpMessage(message.FromId);
                return;
            }

            _inputDataController.

            var reqestObjectMessage = new UserInputDataMessage()
            {
                Data = inputData.Value,
                Type = inputData.Type,
                UserId = message.FromId,
                MessageId = message.Id,
            };
            _actorSelection
                .ActorSelection(Context, ActorsPaths.InputDataHandlerActor.Path)
                .Tell(reqestObjectMessage, Self);

        }

        private void UserRequestHandler(PrivateMessage message)
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
            }
        }

        private InputData TryPullInputDataFromMessage(string inputStr)
        {
            var stringWithOutWiteSpace = inputStr.Replace(" ", "");
            Match match;

            match = _vinCodeRegex.Match(stringWithOutWiteSpace);
            if (match.Success)
            {
                var value = ReplaceCyrillicToLatin(match.Value);
                return new InputData(value, InputDataType.Vin);
            }

            match = _regNumberRegex.Match(stringWithOutWiteSpace);
            if (match.Success)
            {
                var value = ReplaceLatinToCyrillic(match.Value);
                return new InputData(value, InputDataType.LicensePlate);
            }

            // match = _fioRegex.Match(inputStr);
            //if (match.Success)
            //    return new KeyValuePair<InputDataType, string>(InputDataType.FullName, match.Value);

            return null;
        }

        private RequestType DefineRequestType(string stringPayload)
        {
            var payload = JsonConvert.DeserializeObject<RequestPayload>(stringPayload);
            return payload.RequestType;
        }

        private string ReplaceCyrillicToLatin(string inputString)
        {
            var upperCaseText = inputString.ToUpper();

            foreach (var symbol in _latinToCyrillicSymbols)
            {
                upperCaseText = upperCaseText.Replace(symbol.Value, symbol.Key);
            }

            return upperCaseText;
        }

        private string ReplaceLatinToCyrillic(string inputString)
        {
            var upperCaseText = inputString.ToUpper();

            foreach (var symbol in _latinToCyrillicSymbols)
            {
                upperCaseText = upperCaseText.Replace(symbol.Key, symbol.Value);
            }

            return upperCaseText;
        }

        private void SendHelpMessage(int userId)
        {
            _messagesSenderController.SendMessage(userId, StaticResources.HelpMessage);
        }
    }

    public class InputData
    {
        public InputData(string value, InputDataType type)
        {
            Type = type;
            Value = value;
        }

        public InputDataType Type { get; set; }
        public string Value { get; set; }
    }
}
