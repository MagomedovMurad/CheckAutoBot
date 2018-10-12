using Akka.Actor;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageSenderActor: ReceiveActor
    {
        private KeyboardBuilder _keyboardBuilder;
        private IRepositoryFactory _repositoryFactory;



        public PrivateMessageSenderActor()
        {
            _keyboardBuilder = new KeyboardBuilder();
            _repositoryFactory = new RepositoryFactory();


            Receive<HelpMessage>(x => SendHelpMessage(x));
            Receive<SendToUserMessage>(x => SendToUserMessageHandler(x));
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

        private async void SendToUserMessageHandler(SendToUserMessage message)
        {
            var requestTypes = await GetRequestTypes(message.RequestObjectId).ConfigureAwait(false);
            var keyboard = _keyboardBuilder.CreateKeyboard(requestTypes, InputDataType.Vin); //TODO: тип входных данных

            SendMessage(message.UserId, message.Text, keyboard);
        }

        private void SendHelpMessage(HelpMessage msg)
        {
            string message = $"Не удалось распознать запрос!{Environment.NewLine}" +
                $"Для получения информации введите гос.номер, вин код или ФИО.{Environment.NewLine}" +
                $"Примеры:{Environment.NewLine}" +
                $"XWB3K32EDCA235494{Environment.NewLine}" +
                $"Р927УТ38{Environment.NewLine}" +
                $"Иванов Иван Иванович{Environment.NewLine}";

            SendMessage(msg.UserId, message);
        }

        private void SendMessage(int userId, string text, Keyboard keyboard = null, string attachments = null)
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

        /// <summary>
        /// Получить типы выполненных запросов
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<RequestType>> GetRequestTypes(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetRequestTypes(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
