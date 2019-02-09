using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.YandexMoneyModels
{
    public class Payment
    {
        /// <summary>
        /// Тип уведомления (перевод с кошелька/карты)
        /// </summary>
        public string NotificationType { get; set; }

        /// <summary>
        /// Идентификатор операции в истории счета получателя
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Сумма операции
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Сумма, которая списана со счета отправителя
        /// </summary>
        public string WithdrawAmount { get; set; }

        /// <summary>
        /// Код валюты счета пользователя. Всегда 643 (рубль РФ согласно ISO 4217)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Дата и время совершения перевода
        /// </summary>
        public string Datetime { get; set; }

        /// <summary>
        /// Для переводов из кошелька - номер счета отправителя.
        /// Для переводов с произвольной карты - параметр содержит пустую строку
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Для переводов из кошелька — перевод защищен кодом протекции.
        /// Для переводов с произвольной карты — всегда false
        /// </summary>
        public bool Codepro { get; set; }

        /// <summary>
        /// Метка платежа. Если метки у платежа нет, параметр содержит пустую строку
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// SHA-1 hash параметров уведомления
        /// </summary>
        public string Sha1Hash { get; set; }

        /// <summary>
        /// Флаг означает, что уведомление тестовое. По умолчанию параметр отсутствует
        /// </summary>
        public bool TestNotification { get; set; }

        /// <summary>
        /// Флаг означает, что пользователь не получил перевод. Возможные причины:
        /// Перевод заморожен, так как на счете получателя достигнут лимит доступного остатка.Отображается в поле hold ответа метода account-info.
        /// Перевод защищен кодом протекции. В этом случае codepro = true.
        /// </summary>
        public bool Unaccepted { get; set; }
    }
}
