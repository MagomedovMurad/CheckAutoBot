using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.YandexMoneyModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CheckAutoBot.Controllers
{
    public class YandexMoneyController
    {
        private DbQueryExecutor _queryExecutor;
        private MessagesSenderController _messagesSenderController;
        private ICustomLogger _logger;

        public YandexMoneyController(DbQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        public async Task HandlePayment(Payment payment, bool isValid)
        {
            var requestObjectId = int.Parse(payment.Label);
            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            if (isValid)
            {
                var auto = requestObject as Auto;
                var data = auto.LicensePlate == null ? $"по VIN коду {auto.Vin}" : $"по гос. номеру {auto.LicensePlate}";
                var text = $"Запрос {data} оплачен. Вы можете выполнить следующий запрос.";
                _messagesSenderController.SendMessage(requestObject.UserId, text);

                var textForAdmin = $"Оплачен запрос ({requestObject.Id}) пользователем с ID *id{requestObject.UserId}";
                _logger.WriteToLog(LogLevel.Warn, textForAdmin, true);

                await _queryExecutor.MarkAsPaid(requestObjectId);
            }
            else
            {
                var textForAdmin = $"Не удалось оплатить запрос ({requestObject.Id}) пользователем с ID *id{requestObject.UserId}";
                _logger.WriteToLog(LogLevel.Warn, textForAdmin, true);
            }
        }
    }
}
