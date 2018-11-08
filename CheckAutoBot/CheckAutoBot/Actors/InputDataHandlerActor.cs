﻿using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class InputDataHandlerActor: ReceiveActor
    {
        public InputDataHandlerActor()
        {
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
        }

        private async Task<bool> UserInputDataMessageHandler(UserInputDataMessage message)
        {
            try
            {
                RequestObject data;

                switch (message.Type)
                {
                    #region VIN
                    case InputDataType.Vin:
                        {
                            data = new Auto
                            {
                                Vin = message.Data,
                                Date = message.Date,
                                UserId = message.UserId,
                                MessageId = message.MessageId
                            };

                            
                        }
                        break;
                    #endregion VIN

                    #region LicensePlate
                    case InputDataType.LicensePlate:
                        data = new Auto
                        {
                            LicensePlate = message.Data,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;

                    #endregion LicensePlate

                    #region FullName
                    case InputDataType.FullName:

                        string[] personData = message.Data.Split(' ');
                        string lastName = personData[0].Replace('_', ' '); //Фамилия
                        string firstName = personData[1].Replace('_', ' '); //Имя
                        string middleName = personData[2].Replace('_', ' '); //Отчество
                        data = new Person
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            MiddleName = middleName,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion FullName

                    default:
                        throw new InvalidOperationException($"Не найден обработчик для типа {message.Type}");
                }

                await _queryExecutor.AddRequestObject(data);

                var text = $"{message.Data}{Environment.NewLine}Выберите доступные действия...";
                var keyboard = await CreateKeyBoard(data).ConfigureAwait(false);
                var msg = new SendToUserMessage(keyboard, message.UserId, text, null);

                _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(msg, Self);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(requestTypes, requestObject.GetType());
        }
    }
}