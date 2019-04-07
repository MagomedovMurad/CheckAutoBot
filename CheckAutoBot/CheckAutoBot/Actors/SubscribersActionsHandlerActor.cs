using Akka.Actor;
using CheckAutoBot.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class SubscribersActionsHandlerActor: ReceiveActor
    {
        private ActorSelector _actorSelection;

        public SubscribersActionsHandlerActor()
        {
            ReceiveAsync<MessagesAllowedEventMessage>(x => MessagesAllowedEventHandler(x));
        }
        protected override void PreStart()
        {
            _actorSelection = new ActorSelector();
        }

        private async Task MessagesAllowedEventHandler(MessagesAllowedEventMessage message)
        {
            var helpMessage = new SendToUserMessage() { UserId = message.UserId, Text = GetRules() };
            _actorSelection
                .ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path)
                .Tell(helpMessage, Self);
        }

        private string GetRules()
        {
            return "📝 Правила пользования ботом:" + (Environment.NewLine + Environment.NewLine) +

                    "🚗 Вы можете произвести следующие виды проверок:" + Environment.NewLine +
                    "─ История регистрации в ГИБДД " + Environment.NewLine +
                    "─ ДТП" + Environment.NewLine +
                    "─ Наличие ограничений " + Environment.NewLine +
                    "─ Нахождение в розыске" + Environment.NewLine +
                    "─ Нахождение в залоге" + (Environment.NewLine + Environment.NewLine) +

                    "💡 Для получения информации об автомобиле: " + Environment.NewLine +
                    "👉🏻 1.Пришлите гос.номер или VIN код. " + Environment.NewLine +
                    "Примеры: " + Environment.NewLine +
                    "─ С065MK78" + Environment.NewLine +
                    "─ WVWZZZ16ZDM065881" + Environment.NewLine +
                    "👉🏻 2.Выберите доступное действие на панели кнопок" + (Environment.NewLine + Environment.NewLine) +

                    "💰 Оплата: " + Environment.NewLine +
                    "• Бот работает по системе ПОСТОПЛАТЫ." + Environment.NewLine +
                    "• Стоимость всей информации по одному автомобилю составляет 3&#8419;8&#8419; рублей." + Environment.NewLine +
                    "• Запрос оплачивается, если выполнен хотя бы один вид проверки. " + Environment.NewLine +
                    "• После выполнения всех проверок Вы получите ссылку для оплаты." + Environment.NewLine +
                    "• Оплата проводится через сервис онлайн - платежей Яндек.Деньги." + (Environment.NewLine + Environment.NewLine) +

                    "💵 Способы оплаты: " + Environment.NewLine +
                    "1.Банковская карта 💳 " + Environment.NewLine +
                    "2.Яндекс.Кошелек 👛 ";
        }
    }
}
