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
        private readonly ILogger _logger;
        private readonly VkApiManager _vkApi;

        public PrivateMessageSenderActor(ILogger logger, VkApiManager vkApi)
        {
            _logger = logger;
            _vkApi = vkApi;
            ReceiveAsync<SendToUserMessage>(message => SendToUserMessageHandler(message));
        }

        private async Task SendToUserMessageHandler(SendToUserMessage message)
        {
            var attachments = PhotoToAttachment(message.UserId, message.Photo);
            _vkApi.SendMessage(message.UserId, message.Text, attachments, message.Keyboard);
        }

        private string PhotoToAttachment(int userId, byte[] binPhoto)
        {
            if (binPhoto == null)
                return null;

            var photo = _vkApi.UploadPhoto(userId, binPhoto);
            return $"photo{photo.OwnerId}_{photo.Id}";
        }
    }
}
