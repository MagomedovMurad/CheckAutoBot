using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.YandexMoneyModels
{
    public class QuickpayForm
    {
        #region  Обязательные параметры

        /// <summary>
        /// Номер кошелька в Яндекс.Деньгах, на который нужно зачислять деньги отправителей.
        /// </summary>
        public string Receiver { get; set; }

        public string QuickpayFormType { get; set; }

        /// <summary>
        /// Назначение платежа. 
        /// До 150 символов!
        /// </summary>
        public string Targets { get; set; }

        /// <summary>
        /// Способ оплаты.
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// Сумма перевода (спишется с отправителя).
        /// </summary>
        public int Sum { get; set; }

        #endregion  Обязательные параметры

        #region Необязательные параметры
        /// <summary>
        /// Название перевода в истории отправителя (для переводов из кошелька или с привязанной карты). 
        /// Отображается в кошельке отправителя. 
        /// До 50 символов!
        /// </summary>
        public string Formcomment { get; set; }

        /// <summary>
        /// Название перевода на странице подтверждения. 
        /// Рекомендуем делать его таким же, как formcomment.
        /// </summary>
        public string ShortDest { get; set; }

        /// <summary>
        /// Метка, которую сайт или приложение присваивает конкретному переводу. 
        /// Например, в качестве метки можно указывать код или идентификатор заказа. 
        /// До 64 символов!
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Поле, в котором можно передать комментарий отправителя перевода.
        /// До 200 символов!
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// URL-адрес для редиректа после совершения перевода.
        /// </summary>
        public string SuccessURL { get; set; }

        /// <summary>
        /// Нужны ФИО отправителя.
        /// </summary>
        public bool NeedFio { get; set; }

        /// <summary>
        /// ужна электронная почты отправителя.
        /// </summary>
        public bool NeedEmail { get; set; }

        /// <summary>
        /// Нужен телефон отправителя.
        /// </summary>
        public bool NeedPhone { get; set; }

        /// <summary>
        /// Нужен адрес отправителя.
        /// </summary>
        public bool NeedAddress { get; set; }

        #endregion Необязательные параметры
    }
}
