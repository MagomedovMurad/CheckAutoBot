using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CheckAutoBot.Controllers
{
    public interface IPrivateMessagesController
    {
        void HandleMessage(PrivateMessage message);
    }

    public class PrivateMessagesController: IPrivateMessagesController
    {
        private readonly ICustomLogger _logger;
        private DbQueryExecutor _queryExecutor;
        private IMessagesSenderController _messagesSenderController;
        private IInputDataController _inputDataController;
        private IUserRequestController _userRequestController;

        private Regex _regNumberRegex;
        private Regex _vinCodeRegex;
        //private Regex _fioRegex;

        public PrivateMessagesController(IMessagesSenderController messagesSenderController, 
                                         IInputDataController inputDataController,
                                         IUserRequestController userRequestController, 
                                         DbQueryExecutor queryExecutor,
                                         ICustomLogger logger)
        {
            _messagesSenderController = messagesSenderController;
            _inputDataController = inputDataController;
            _userRequestController = userRequestController;
            _logger = logger;
            _queryExecutor = queryExecutor;

            Initialize();
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

            var dateTime = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(message.Date);
            _inputDataController.HandleInputData(inputData, message.FromId, message.Id, dateTime);
        }

        private void UserRequestHandler(PrivateMessage message)
        {
            var payloadEnvelop = JsonConvert.DeserializeObject<PayloadEnvelop>(message.Payload);
            var payload = JsonConvert.DeserializeObject(payloadEnvelop.Payload, Type.GetType(payloadEnvelop.DotNetType));

            if (payload is RequestPayload requestPayload)
            {
                var dateTime = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(message.Date);
                _userRequestController.HandleUserRequest(message.Id, message.FromId, requestPayload.RequestType, dateTime);
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
