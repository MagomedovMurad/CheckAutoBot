using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Enums
{
    public enum ActionType
    {
        /// <summary>
        /// Получить номер полиса ОСАГО
        /// </summary>
        PolicyNumber,

        /// <summary>
        /// Получить информацию о полисе ОСАГО
        /// </summary>
        PolicyInfo,

        /// <summary>
        /// Получить историю автомобиля
        /// </summary>
        History,

        /// <summary>
        /// Получить ДТП автомобиля
        /// </summary>
        Dtp,

        /// <summary>
        /// Получить информацию о наличия автомобиля в розыске 
        /// </summary>
        Wanted,

        /// <summary>
        /// Получить информацию о наличии на автомобиле ограничений
        /// </summary>
        Restricted,

        /// <summary>
        /// Получить информацию о нахождении автомобиля в залоге
        /// </summary>
        Pledge,

        /// <summary>
        /// Проверка действительности паспорта человека
        /// </summary>
        CheckUserPassport,

        /// <summary>
        /// Получить информацию о наличии долгов у человека
        /// </summary>
        CheckUserArrears,

        /// <summary>
        /// Получить диагностическую карту
        /// </summary>
        DiagnosticCard,

        /// <summary>
        /// Получить вин код по дигностической карте
        /// </summary>
        VinByDiagnosticCard,

        /// <summary>
        /// Получить вин код по полису ОСАГО
        /// </summary>
        VinByPolicy

    }
}
