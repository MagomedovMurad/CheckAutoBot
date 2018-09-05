using Akka.Actor;
using CheckAutoBot.Storage;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageHandlerActor: ReceiveActor
    {
        Regex _regNumberRussianSymbolsRegex;
        Regex _regNumberLatinSymbolsRegex;
        Regex _vinCodeRegex;
        Regex _fioRegex;

        public PrivateMessageHandlerActor()
        {
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
            if (message.Payload == null)
            {
                var requestObjectTypeWithValue = DefineRequestObjectType(message.Text);
                if (requestObjectTypeWithValue.HasValue)
                {
                    var reqestObject = new RequestObject()
                    {
                        Data = requestObjectTypeWithValue.Value.Value,
                        Type = requestObjectTypeWithValue.Value.Key,
                        UserId = message.FromId,
                        Date = DateTime.Now
                    };

                    //SaveToDb

                    //Return new PrivateMessage with buttons

                }
                else
                {
                   
                }
            }
            else
            {
                
            }
        }

        private void AddRequestObject()
        {
            var requestObject = new RequestObject()
            {

            };
        }

        private KeyValuePair<RequestObjectType, string>? DefineRequestObjectType(string inputStr)
        {
            Match match = _regNumberRussianSymbolsRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<RequestObjectType, string>(RequestObjectType.LicensePlate, match.Value);

            match = _regNumberLatinSymbolsRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<RequestObjectType, string>(RequestObjectType.LicensePlate, match.Value);

            match = _vinCodeRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<RequestObjectType, string>(RequestObjectType.Vin, match.Value);

            match = _fioRegex.Match(inputStr);
            if (match.Success)
                return new KeyValuePair<RequestObjectType, string>(RequestObjectType.FullName, match.Value);

            return null;
        }
    }
}
