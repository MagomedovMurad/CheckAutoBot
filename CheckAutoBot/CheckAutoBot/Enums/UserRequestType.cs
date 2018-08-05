using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Enums
{
    public enum UserRequestType
    {
        /// <summary>
        /// Запрос вин кода
        /// </summary>
        Vin,

        /// <summary>
        /// Запрос истории автомобиля
        /// </summary>
        History,

        /// <summary>
        /// Запрос ДТП автомобиля
        /// </summary>
        Dtp,

        /// <summary>
        /// Запрос наличия в розыске автомобиля
        /// </summary>
        Wanted,

        /// <summary>
        /// Запрос наличия на автомобиле ограничений
        /// </summary>
        Restricted,

        /// <summary>
        /// Запрос наличия автомобиля в залоге
        /// </summary>
        Pledge,

        /// <summary>
        /// Проверка действительности паспорта человека
        /// </summary>
        CheckUserPassport,

        /// <summary>
        /// Проверка наличия долгов человека
        /// </summary>
        CheckUserArrears


    }
}
