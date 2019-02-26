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
        OsagoVechicle,

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
        /// Получить данные по ПТС
        /// </summary>
        VechiclePasportData,

        /// <summary>
        /// Получить периоды владения
        /// </summary>
        OwnershipPeriods

    }
}
