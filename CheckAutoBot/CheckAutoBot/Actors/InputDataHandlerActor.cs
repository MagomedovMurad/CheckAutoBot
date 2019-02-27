using Akka.Actor;
using CheckAutoBot.Api;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api;
using CheckAutoBot.Vk.Api.MessagesModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class InputDataHandlerActor: ReceiveActor
    {
        private readonly DbQueryExecutor _queryExecutor;
        private readonly ICanSelectActor _actorSelector;
        private readonly KeyboardBuilder _keyboardBuilder;
        private readonly ILogger _logger;
        private readonly VkApiManager _vkApi;

        public InputDataHandlerActor(ILogger logger, DbQueryExecutor queryExecutor, VkApiManager vkApi)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _actorSelector = new ActorSelector();
            _keyboardBuilder = new KeyboardBuilder();
            _vkApi = vkApi;
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
        }

        private async Task<bool> UserInputDataMessageHandler(UserInputDataMessage message)
        {
            try
            {
                // var isSubscriber = Groups.IsMember("checkautobot", message.UserId.ToString(), "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2");
                var isSubscriber = _vkApi.UserIsMember("checkautobot", message.UserId);

                if (!isSubscriber)
                {
                    SendMessageToUser(null, message.UserId, StaticResources.OnlySubscribers);
                    return true;
                }

                if (!await Test(message.UserId))
                    return true;

                RequestObject data;

                switch (message.Type)
                {
                    #region VIN
                    case InputDataType.Vin:
                        {
                            data = new Auto
                            {
                                Vin = message.Data,
                                Date = message.Date,
                                UserId = message.UserId,
                                MessageId = message.MessageId
                            };

                            await _queryExecutor.AddRequestObject(data);

                            var msg = new StartGeneralInfoSearchMessage()
                            {
                                RequestObjectId = data.Id,
                                Vin = message.Data
                            };
                            _actorSelector.ActorSelection(Context, ActorsPaths.VinCodeHandlerActor.Path).Tell(msg, Self);

                            //Send buttons to user
                            //var keyboard = await CreateKeyBoard(data);
                            //var text = $"VIN код: {(data as Auto).Vin}. {Environment.NewLine}" +
                            //           $"Выберите доступное действие.";
                            //SendMessageToUser(keyboard, data.UserId, text);
                        }
                        break;
                    #endregion VIN

                    #region LicensePlate
                    case InputDataType.LicensePlate:
                        {
                            data = new Auto
                            {
                                LicensePlate = message.Data,
                                Date = message.Date,
                                UserId = message.UserId,
                                MessageId = message.MessageId
                            };
                            await _queryExecutor.AddRequestObject(data);

                            var msg = new StartVinSearchingMessage(data.Id);
                            _actorSelector.ActorSelection(Context, ActorsPaths.LicensePlateHandlerActor.Path).Tell(msg, Self);

                            var text = $"⌛ Выполняется проверка возможности получения информации по гос. номеру {message.Data}.{Environment.NewLine}" +
                                       $"Дождитесь ответа.{Environment.NewLine}" +
                                       $"Это займет не более 2-х минут";

                            SendMessageToUser(null, message.UserId, text);

                            break;
                        }

                    #endregion LicensePlate

                    #region FullName
                    case InputDataType.FullName:

                        string[] personData = message.Data.Split(' ');
                        string lastName = personData[0].Replace('_', ' '); //Фамилия
                        string firstName = personData[1].Replace('_', ' '); //Имя
                        string middleName = personData[2].Replace('_', ' '); //Отчество
                        data = new Person
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            MiddleName = middleName,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion FullName

                    default:
                        throw new InvalidOperationException($"Не найден обработчик для типа {message.Type}");
                }

                //await _queryExecutor.AddRequestObject(data);


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<Keyboard> CreateKeyBoard(RequestObject requestObject)
        {
            //var requestTypes = await _queryExecutor.GetExecutedRequestTypes(requestObject.Id).ConfigureAwait(false);
            return _keyboardBuilder.CreateKeyboard(new List<RequestType>(), requestObject.GetType());
        }

        private async Task<bool> Test(int userId)
        {
            var lastRequestObject = await _queryExecutor.GetLastUserRequestObject(userId);
            if (lastRequestObject == null)
                return true;
            var existRequestsInProcess = await _queryExecutor.ExistRequestsInProcess(lastRequestObject.Id);
            if (existRequestsInProcess)
            {
                var text = "Дождитесь завершения выполнения запроса";
                SendMessageToUser(null, userId, text);
                return false;
            }

            var succesfullComletedRequests = await _queryExecutor.GetExecutedRequestTypes(lastRequestObject.Id);

            if (succesfullComletedRequests.Any() && !lastRequestObject.IsPaid)
            {
                var auto = lastRequestObject as Auto;
                var autoData = auto.LicensePlate != null ? auto.LicensePlate : auto.Vin;

                var data = auto.LicensePlate != null ? $"гос. номеру {autoData}" : $"VIN коду {autoData}";
                var paylink = YandexMoney.GenerateQuickpayUrl(autoData, auto.Id.ToString());
                var text = $"💵 Оплатите предыдущий запрос по {data}. {Environment.NewLine}" +
                           $"Для оплаты перейдите по ссылке:{Environment.NewLine}" +
                           $"{paylink}{Environment.NewLine}";
                if(succesfullComletedRequests.Count() < 5)
                    text = text + $"Или выберите доступное действие для {autoData}.";
                var keyboard = _keyboardBuilder.CreateKeyboard(succesfullComletedRequests, typeof(Auto));
                SendMessageToUser(keyboard, userId, text);
                return false;
            }

            return true;
        }

        private void SendMessageToUser(Keyboard keyboard, int userId, string text)
        {
            var msg = new SendToUserMessage(userId, text, keyboard: keyboard);
            _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(msg, Self);
        }

    }
}
