using CheckAutoBot.Api;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CheckAutoBot.Controllers
{
    public interface IInputDataController
    {
        void HandleInputData(InputData inputData, int userId, int messageId, DateTime date);
    }

    public class InputDataController: IInputDataController
    {
        private readonly DbQueryExecutor _queryExecutor;
        private readonly KeyboardBuilder _keyboardBuilder;
        private readonly ICustomLogger _logger;

        private IVinCodeController _vinCodeController;
        private ILicensePlateController _licensePlateController;
        private IMessagesSenderController _messagesSenderController;

        public InputDataController(ICustomLogger logger, 
                                   DbQueryExecutor queryExecutor, 
                                   IVinCodeController vinCodeController, 
                                   ILicensePlateController licensePlateController,
                                   IMessagesSenderController messagesSenderController)
        {
            _logger = logger;
            _queryExecutor = queryExecutor;
            _keyboardBuilder = new KeyboardBuilder();
            _vinCodeController = vinCodeController;
            _licensePlateController = licensePlateController;
            _messagesSenderController = messagesSenderController;
        }

        public void HandleInputData(InputData inputData, int userId, int messageId, DateTime date)
        {
            try
            {
                // var isSubscriber = Groups.IsMember("checkautobot", message.UserId.ToString(), "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2");
                //var isSubscriber = _vkApi.UserIsMember("checkautobot", message.UserId);

                //if (!isSubscriber)
                //{
                //    SendMessageToUser(null, message.UserId, StaticResources.OnlySubscribers);
                //    return true;
                //}

                if (!CheckUser(userId, inputData))
                    return;

                switch (inputData.Type)
                {
                    #region VIN
                    case InputDataType.Vin:
                        {
                            var auto = SaveAutoWithVinToDB(userId, messageId, inputData.Value, date);
                            _vinCodeController.StartGeneralInfoSearch(inputData.Value, auto.Id);
                            var message = GetMessageForUser(inputData);
                            _messagesSenderController.SendMessage(userId, message);
                        }
                        break;
                    #endregion VIN

                    #region LicensePlate
                    case InputDataType.LicensePlate:
                        {
                            var auto = SaveAutoWithLPToDB(userId, messageId, inputData.Value, date);

                            _licensePlateController.StartVinSearch(inputData.Value, auto.Id);
                            var message = GetMessageForUser(inputData);
                            _messagesSenderController.SendMessage(userId, message);

                        }
                        break;
                    #endregion LicensePlate

                    #region FullName
                    case InputDataType.FullName:

                        //string[] personData = message.Data.Split(' ');
                        //string lastName = personData[0].Replace('_', ' '); //Фамилия
                        //string firstName = personData[1].Replace('_', ' '); //Имя
                        //string middleName = personData[2].Replace('_', ' '); //Отчество
                        //requestObject = new Person
                        //{
                        //    FirstName = firstName,
                        //    LastName = lastName,
                        //    MiddleName = middleName,
                        //    Date = message.Date,
                        //    UserId = message.UserId,
                        //    MessageId = message.MessageId
                        //};
                        break;
                    #endregion FullName

                    default:
                        throw new InvalidOperationException($"Не найден обработчик для типа {inputData.Type}");
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.WriteToLog(LogLevel.Error, $"Ошибка пр обработке входных данных: {ex}");
            }
        }

        private Auto SaveAutoWithVinToDB(int userId, int messageId, string vin, DateTime date)
        {
            var auto = new Auto
            {
                Vin = vin,
                Date = date,
                UserId = userId,
                MessageId = messageId
            };

            _queryExecutor.AddRequestObject(auto);
            return auto;
        }

        private Auto SaveAutoWithLPToDB(int userId, int messageId, string licensePlate, DateTime date)
        {
            var auto = new Auto
            {
                LicensePlate = licensePlate,
                Date = date,
                UserId = userId,
                MessageId = messageId
            };
            _queryExecutor.AddRequestObject(auto);
            return auto;
        }

        private string GetMessageForUser(InputData data)
        {

           var dataType = GetStringInputDataType(data.Type);
           return $"⌛ Выполняется проверка возможности получения информации по {dataType}: {data.Value}.{Environment.NewLine}" +
                       $"Дождитесь ответа.{Environment.NewLine}" +
                       $"Обычно это занимает не более 2-х минут";
        }

        private string GetStringInputDataType(InputDataType type)
        {
            switch (type)
            {
                case InputDataType.Vin:
                    return "VIN коду";
                case InputDataType.LicensePlate:
                    return "гос. номеру";
                default: return null;
            }
        }

        private bool CheckUser(int userId, InputData inputdata)
        {
            var lastRequestObject = _queryExecutor.GetLastUserRequestObject(userId);
            if (lastRequestObject is null)
                return true;

            var existRequestsInProcess = _queryExecutor.ExistRequestsInProcess(lastRequestObject.Id);
            if (existRequestsInProcess)
            {
                var message = "⛔ Дождитесь завершения выполнения запроса";
                _messagesSenderController.SendMessage(userId, message);
                return false;
            }

            var requestTypes = _queryExecutor.GetExecutedRequestTypes(lastRequestObject.Id);

            if (requestTypes.Any() && !lastRequestObject.IsPaid)
            {
                if (lastRequestObject is Auto auto)
                    AutoRequestsPayHandler(auto, inputdata, requestTypes);

                return false;
            }

            return true;
        }

        private void AutoRequestsPayHandler(Auto auto, InputData inputData, IEnumerable <RequestType> requestTypes)
        {
            var dataType = GetStringInputDataType(inputData.Type);

            var paylink = YandexMoney.GenerateQuickpayUrl(inputData.Value, auto.Id.ToString());

            var message = $"💵 Оплатите предыдущий запрос по {dataType}: {inputData.Value} (3&#8419;8&#8419; руб.). {Environment.NewLine}" +
                          $"Для оплаты перейдите по ссылке:{Environment.NewLine}" +
                          $"{paylink}{Environment.NewLine}";

            if (requestTypes.Count() < 6)
                message = message + $"Или выберите доступное действие для {inputData.Value}.";
            var keyboard = _keyboardBuilder.CreateKeyboard(typeof(Auto), requestTypes);

            _messagesSenderController.SendMessage(auto.UserId, message, keyboard: keyboard);
        }
    }
}
