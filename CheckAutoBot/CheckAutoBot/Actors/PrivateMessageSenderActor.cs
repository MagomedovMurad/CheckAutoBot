using Akka.Actor;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api;
using CheckAutoBot.Vk.Api.MessagesModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageSenderActor: ReceiveActor
    {
        private ILogger _logger;

        private const string accessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2";

        public PrivateMessageSenderActor(ILogger logger)
        {
            _logger = logger;

            //Receive<HelpMessage>(x => SendHelpMessage(x));
            ReceiveAsync<SendToUserMessage>(x => SendToUserMessageHandler(x));
        }

        private async Task SendToUserMessageHandler(SendToUserMessage message)
        {
            var requestTypes = new List<RequestType>();

            var attachments = PhotoToAttachment(message.UserId, message.Photo);
            SendMessage(message.UserId, message.Text, message.Keyboard, attachments);
        }

        private string PhotoToAttachment(int userId, byte[] photoBin)
        {
            if (photoBin == null)
                return null;

            var uploadServerResponse = Photos.GetMessagesUploadServer(userId.ToString(), accessToken);
            var uploadPhotoResponse = Photos.UploadPhotoToServer(uploadServerResponse.UploadUrl, photoBin);
            var photo = Photos.SaveMessagesPhoto(uploadPhotoResponse, accessToken);
            return $"photo{photo.OwnerId}_{photo.Id}";
        }

        //private void SendHelpMessage(HelpMessage msg)
        //{
        //    string message = $"Не удалось распознать запрос!{Environment.NewLine}" +
        //        $"Для получения информации введите гос.номер, вин код или ФИО.{Environment.NewLine}" +
        //        $"Примеры:{Environment.NewLine}" +
        //        $"XWB3K32EDCA235494{Environment.NewLine}" +
        //        $"Р927УТ38{Environment.NewLine}" +
        //        $"Иванов Иван Иванович{Environment.NewLine}";

        //    SendMessage(msg.UserId, message);
        //}

        private void SendMessage(int userId, string text, Keyboard keyboard = null, string attachments = null)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.Error(ex, "Произошла ошибка при отправке сообщения пользователю");
            }
        }

    }
}
