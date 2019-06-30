using CheckAutoBot.Vk.Api;
using CheckAutoBot.Vk.Api.MessagesModels;
using CheckAutoBot.Vk.Api.PhotosModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Utils
{
    public class VkApiManager
    {
        private readonly string _accessToken;
        public VkApiManager(string accessToken)
        {
            _accessToken = accessToken;
        }

        #region Groups

        public bool UserIsMember(string groupId, int userId)
        {
            return Groups.IsMember(groupId, userId, _accessToken);
        }

        #endregion Groups

        #region Photos

        public Photo UploadPhoto(int userId, byte[] binPhoto)
        {
            var uploadServerResponse = Photos.GetMessagesUploadServer(userId.ToString(), _accessToken);
            var uploadPhotoResponse = Photos.UploadPhotoToServer(uploadServerResponse.UploadUrl, binPhoto);
            return Photos.SaveMessagesPhoto(uploadPhotoResponse, _accessToken);
        }

        #endregion Photos

        #region Messages

        public async Task SendMessage(int userId, string text, string attachments, Keyboard keyboard)
        {
            var messageParams = new SendMessageParams()
            {
                Message = text,
                PeerId = userId,
                AccessToken = _accessToken,
                Attachments = attachments,
                Keyboard = keyboard
            };

            await Vk.Api.Messages.Send(messageParams);
        }

        #endregion Messages
    }
}
