using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public class StaticResources
    {
        public static string RequestFailedError = $"🚧 Извините, в данный момент источник данных недоступен.{Environment.NewLine}" +
                                                  $"Пожалуйста, повторите Ваш запрос позднее.{Environment.NewLine}";

        public static string UnexpectedError = $"🚧 Извините, в процессе выполнения запроса произошла непредвиденная ошибка.{Environment.NewLine}" +
                                               $"Информация об ошибке передана разработчикам.{Environment.NewLine}" +
                                               $"Попробуйте повторить Ваш запрос";

        public static string VinNotFoundError = $"К сожалению не удалось найти информацию по гос. номеру.{Environment.NewLine}" +
                                           $" Попробуйте выполнить поиск по vin коду";

        public static string HelpMessage = $"⚠ Не удалось распознать запрос!{Environment.NewLine}" +
                $"💡 Для получения информации введите гос.номер или VIN код.{Environment.NewLine}" +
                $"Примеры:{Environment.NewLine}" +
                $"─ XWB3K32EDCA235494{Environment.NewLine}" +
                $"─ Р927УТ38{Environment.NewLine}";

        public static string OnlySubscribers = "⛔ Только подписчики сообщества могут выполнять запросы. *checkautobot (Подписаться)";

        public static int MyUserId = 192287910;



    }
}
