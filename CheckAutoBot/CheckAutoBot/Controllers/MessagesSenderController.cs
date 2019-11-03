using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CheckAutoBot.Controllers
{
    public interface IMessagesSenderController
    {
        Task SendMessage(int userId, string text, byte[] photo = null, Keyboard keyboard = null);
    }
    public class MessagesSenderController: IMessagesSenderController
    {
        private readonly ICustomLogger _logger;
        private readonly VkApiManager _vkApi;

        public MessagesSenderController(ICustomLogger logger, VkApiManager vkApi)
        {
            _logger = logger;
            _vkApi = vkApi;
        }

        public async Task SendMessage(int userId, string text, byte[] photo = null, Keyboard keyboard = null)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Отправка сообщения: {text}");
            var attachments = PhotoToAttachment(userId, photo);
            await _vkApi.SendMessage(userId, text, attachments, keyboard);
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
