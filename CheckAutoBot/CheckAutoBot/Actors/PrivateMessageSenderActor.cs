using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageSenderActor: ReceiveActor
    {
        public PrivateMessageSenderActor()
        {
            Receive<HelpMessage>(x => SendHelpMessage(x));
            Receive<UserRequestMessage>(x => UserRequestMessageHandler(x));
            Receive<UserRequestObjectMessage>(x => UserRequestObjectMessageHandler(x));
        }

        private void UserRequestMessageHandler(UserRequestMessage message)
        {
            message.
        }

        private void UserRequestObjectMessageHandler(UserRequestObjectMessage message)
        {

        }


        private void SendHelpMessage(HelpMessage msg)
        {
            string message = $"Не удалось распознать запрос!{Environment.NewLine}" +
                $"Для получения иформации введите гос.номер, вин код или ФИО.{Environment.NewLine}" +
                $"Прмеры:{Environment.NewLine}" +
                $"XWB3K32EDCA235494{Environment.NewLine}" +
                $"Р927УТ38{Environment.NewLine}" +
                $"Иванов Иван Иванович{Environment.NewLine}";

            var messageParams = new SendMessageParams()
            {
                Message = message,
                PeerId = msg.UserId
            };

            Vk.Api.Messages.Send(messageParams);
        }
    }
}
