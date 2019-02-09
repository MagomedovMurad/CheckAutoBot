using Akka.Actor;
using CheckAutoBot.Api;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.YandexMoneyModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckAutoBot.Actors
{
    public class YandexMoneyRequestHandlerActor : ReceiveActor
    {
        private DbQueryExecutor _queryExecutor;
        private ICanSelectActor _actorSelector;
        public YandexMoneyRequestHandlerActor(DbQueryExecutor queryExecutor)
        {
            _actorSelector = new ActorSelector();
            _queryExecutor = queryExecutor;
            ReceiveAsync<PaymentRequest>(x => YandexMoneyMessageHandler(x));
        }

        private async Task YandexMoneyMessageHandler(PaymentRequest payment)
        {
            var requestObjectId = int.Parse(payment.Payment.Label);
            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            if (payment.IsValid)
            {
                var auto = requestObject as Auto;
                var data = auto.LicensePlate == null ? $"по VIN коду {auto.Vin}" : $"по гос. номеру {auto.LicensePlate}";
                var text = $"Запрос {data} оплачен. Вы можете выполнить следующий запрос.";
                var message = new SendToUserMessage(requestObject.UserId, text);
                _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(message, Self);

                var textForAdmin = $"Оплачен запрос ({requestObject.Id}) пользователем с ID *id{requestObject.UserId}";
                var messageForAdmin = new SendToUserMessage(StaticResources.MyUserId, textForAdmin);
                _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(messageForAdmin, Self);
                await _queryExecutor.MarkAsPaid(requestObjectId);
            }
            else
            {
                var textForAdmin = $"Не удалось оплатить запрос ({requestObject.Id}) пользователем с ID *id{requestObject.UserId}";
                var messageForAdmin = new SendToUserMessage(requestObject.UserId, textForAdmin);
                _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(messageForAdmin, Self);
            }
        }
    }
}
