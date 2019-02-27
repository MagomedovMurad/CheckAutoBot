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
        private Button _vechiclePassportDataButton { get; set; }
        private Button _ownershipPeriodsButton { get; set; }

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
            _vechiclePassportDataButton = CreateVechiclePassportDataButton();
            _ownershipPeriodsButton = CreateOwnershipPeriodsButton();

            _carRequestTypeWithButton = new Dictionary<RequestType, Button>()
            {
                //{ RequestType.History, _historyButton },
                { RequestType.VechiclePassportData, _vechiclePassportDataButton },
                { RequestType.OwnershipPeriods, _ownershipPeriodsButton},
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
        /// <param name="existRequestTypes">Типы запросов для которых НЕ нужно создавать кнопки</param>
        /// <param name="objectType">Тип объекта</param>
        public Keyboard CreateKeyboard(IEnumerable<RequestType> existRequestTypes, Type objectType)
        {
            Dictionary<RequestType, Button> requestTypeWithButton = null;

            if(objectType.Equals(typeof(Auto)))
                requestTypeWithButton = _carRequestTypeWithButton;

            var buttons = requestTypeWithButton.Where(x => !existRequestTypes.Contains(x.Key)).Select(x => x.Value);
            var buttonsArray = buttons.Select(x => x.ToArray()).ToArray();

            return new Keyboard()
            {
                Buttons = buttonsArray,
                OneTime = true
            };
        }

        private Button CreateHistoryButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.History
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "История регистрации в ГИБДД",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
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

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };


            var action = new ButtonAction()
            {
                Lable = "ДТП",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };
        }

        private Button CreateRestrictedButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Restricted
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "Наличие ограничений",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };
        }

        private Button CreateWantedButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Wanted
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "Нахождение в розыске",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            var dtpButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };

            return dtpButton;

        }

        private Button CreatePledgeButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.Pledge
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "Нахождение в залоге",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            return new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };
        }

        private Button CreatePersonPledgeButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.PersonPledge
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "Проверить долги",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            var dtpButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };

            return dtpButton;

        }

        private Button CreateVechiclePassportDataButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.VechiclePassportData
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "Данные по ПТС",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            var vechiclePassportDataButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };

            return vechiclePassportDataButton;
        }

        private Button CreateOwnershipPeriodsButton()
        {
            var payload = new RequestPayload()
            {
                RequestType = RequestType.OwnershipPeriods
            };

            var envelop = new PayloadEnvelop()
            {
                DotNetType = payload.GetType().ToString(),
                Payload = payload.ToString()
            };

            var action = new ButtonAction()
            {
                Lable = "История регистрации в ГИБДД",
                Type = ButtonActionType.Text,
                Payload = envelop.ToString()
            };

            var ownershipPeriodsButton = new Button()
            {
                Action = action,
                Color = ButtonColor.Primary
            };

            return ownershipPeriodsButton;
        }
    }
}
