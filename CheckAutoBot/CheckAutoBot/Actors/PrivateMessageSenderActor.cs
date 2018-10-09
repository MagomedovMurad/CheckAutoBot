using Akka.Actor;
using CheckAutoBot.GbddModels;
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
                $"Для получения информации введите гос.номер, вин код или ФИО.{Environment.NewLine}" +
                $"Примеры:{Environment.NewLine}" +
                $"XWB3K32EDCA235494{Environment.NewLine}" +
                $"Р927УТ38{Environment.NewLine}" +
                $"Иванов Иван Иванович{Environment.NewLine}";

            SendMessage(message, msg.UserId);
        }

        private string Test(HistoryResult history)
        {
            return $"Марка, модель:  {history.Vehicle.Model} \n" +
                   $"Год выпуска: {history.Vehicle.Year} \n" +
                   $"VIN:  {history.Vehicle.Vin} \n" +
                   $"Кузов:  {history.Vehicle.BodyNumber} \n" +
                   $"Цвет: {history.Vehicle.Color} \n" +
                   $"Рабочий объем(см3):  {history.Vehicle.EngineVolume} \n" +
                   $"Мощность(кВт/л.с.):  {history.Vehicle.PowerHp} \n" +
                   $"Тип:  {history.Vehicle.TypeName} \n" +
                   $"Категория: {history.Vehicle.Category}";
        }

        private void SendMessage(string text, int userId, Keyboard keyboard = null, string attachments = null)
        {
            var accessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2";
            var messageParams = new SendMessageParams()
            {
                Message = text,
                PeerId = userId,
                AccessToken = accessToken,
                Attachments = attachments,
                Keyboard = keyboard
            };

            Vk.Api.Messages.Send(messageParams);
        }
    }
}
