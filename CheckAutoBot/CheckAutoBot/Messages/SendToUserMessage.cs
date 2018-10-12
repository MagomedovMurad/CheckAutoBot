using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class SendToUserMessage
    {
        public SendToUserMessage()
        {

        }

        public SendToUserMessage(int? requestObjectId, int userId, string text, byte[] photo)
        {
            RequestObjectId = requestObjectId;
            UserId = userId;
            Text = text;
            Photo = photo;
        }

        public int UserId { get; set; }

        public int? RequestObjectId { get; set; }

        public string Text { get; set; }

        public byte[] Photo { get; set; }

    }
}
