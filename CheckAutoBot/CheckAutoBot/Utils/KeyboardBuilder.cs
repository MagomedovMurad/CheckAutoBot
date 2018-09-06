using CheckAutoBot.Storage;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class KeyboardBuilder
    {
        private Button _historyButton { get; set; }
        private Button _dtpButton { get; set; }
        private Button _restrictedButton { get; set; }
        private Button _wantedButton { get; set; }
        private Button _pledgeButton { get; set; }
        private Button _personPledgeButton { get; set; }

        private Dictionary<RequestType, Button> _carRequestTypeWithButton { get; set; }
        private Dictionary<RequestType, Button> _personRequestTypeWithButton { get; set; }

        public KeyboardBuilder()
        {

            _historyButton = CreateHistoryButton();
            _dtpButton = CreateDtpButton();
            _restrictedButton = CreateRestrictedButton();
            _wantedButton = CreateWantedButton();
            _pledgeButton = CreatePledgeButton();
            _personPledgeButton = CreatePersonPledgeButton();

            _carRequestTypeWithButton = new Dictionary<RequestType, Button>()
            {
                { RequestType.History, _historyButton },
                { RequestType.Dtp, _dtpButton },
                { RequestType.Restricted, _restrictedButton },
                { RequestType.Wanted, _wantedButton },
                { RequestType.Pledge, _pledgeButton },
            };

            _personRequestTypeWithButton = new Dictionary<RequestType, Button>()
            {
                { RequestType.PersonPledge, _personPledgeButton }
            };
        }

        /// <summary>
        /// Создает клавиатуру
        /// </summary>
        /// <param name="requestTypes">Типы запросов для которых НЕ нужно создавать кнопки</param>
        /// <param name="objectType">Тип объекта</param>
        public Keyboard CreateKeyboard(IEnumerable<RequestType> requestTypes, RequestObjectType objectType)
        {
            Button[] buttons;

            switch (objectType)
            {
                case RequestObjectType.Vin:
                case RequestObjectType.LicensePlate:
                    buttons = _carRequestTypeWithButton.Where(x => requestTypes.Contains(x.Key)).Select(x => x.Value).ToArray();
                    break;
                case RequestObjectType.FullName:
                    buttons = _personRequestTypeWithButton.Where(x => requestTypes.Contains(x.Key)).Select(x => x.Value).ToArray();
                    break;
                default:
                    throw new Exception($"Тип {objectType} не распознан (Class: KeyboardBuilder, Method: CreateKeyboard())");
            }

            return new Keyboard()
            {
                Buttons = new[] { buttons },
                OneTime = true
            };
        }

        private Button CreateHistoryButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.History
            };

            var action = new ButtonAction()
            {
                Lable = "История регистрации в ГИБДД",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };
        }

        private Button CreateDtpButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Dtp
            };

            var action = new ButtonAction()
            {
                Lable = "ДТП",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };
        }

        private Button CreateRestrictedButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Restricted
            };

            var action = new ButtonAction()
            {
                Lable = "Наличие ограничений",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };
        }

        private Button CreateWantedButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Wanted
            };

            var action = new ButtonAction()
            {
                Lable = "Нахождение в розыске",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            var dtpButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };

            return dtpButton;

        }

        private Button CreatePledgeButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Pledge
            };

            var action = new ButtonAction()
            {
                Lable = "Нахождение в залоге",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };
        }

        private Button CreatePersonPledgeButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.PersonPledge
            };

            var action = new ButtonAction()
            {
                Lable = "Проверить долги",
                Type = ButtonActionType.Text,
                Payload = payload.ToString()
            };

            var dtpButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Positive
            };

            return dtpButton;

        }
    }
}
