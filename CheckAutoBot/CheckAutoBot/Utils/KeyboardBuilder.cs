using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class KeyboardBuilder
    {
        private void CreateButton()
        {
            var action = new ButtonAction()
            {
                Lable = "Test key1",
                Type = ButtonActionType.Text,
                Payload = "{\"button\": \"2\"}"
            };

            var button = new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };

            var keyboard = new Keyboard()
            {
                OneTime = true,
                Buttons = new[] { new[] { button } }
            };
        }
    }
}
