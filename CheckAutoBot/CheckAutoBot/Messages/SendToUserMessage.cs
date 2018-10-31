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

        public SendToUserMessage(Keyboard keyboard, int userId, string text, byte[] photo)
        {
            Keyboard = keyboard;
            UserId = userId;
            Text = text;
            Photo = photo;
        }

        public int UserId { get; set; }

        public Keyboard Keyboard { get; set; }

        public string Text { get; set; }

        public byte[] Photo { get; set; }

    }
}
