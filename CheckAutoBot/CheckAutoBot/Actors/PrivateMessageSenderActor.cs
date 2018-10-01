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
            Receive<string>(x => SendMessage(x));
        }

        private void SendMessage(string message)
        {
            //var messageParams = new SendMessageParams()
            //{
            //    Message = message,
            //    PeerId = msg.UserId
            //};

            //Vk.Api.Messages.Send(messageParams);
        }

        private void UserRequestMessageHandler(UserRequestMessage message)
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

            var accessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2";
            var messageParams = new SendMessageParams()
            {
                Message = message,
                PeerId = msg.UserId,
                AccessToken = accessToken
            };

            Vk.Api.Messages.Send(messageParams);
        }
    }
}
