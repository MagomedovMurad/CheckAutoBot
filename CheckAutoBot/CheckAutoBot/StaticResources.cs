using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public class StaticResources
    {
        public static string RequestFailedError = $"При выполнении запроса произошла ошибка.{Environment.NewLine}" +
                                                  $"Источник данных недоступен.{Environment.NewLine}" +
                                                  $"Попробуйте выполнить запрос позднее";

        public static string UnexpectedError = $"Извините, в процессе выполнения запроса произошла непредвиденная ошибка.{Environment.NewLine}" +
                                               $"Информация об ошибке передана разработчикам.{Environment.NewLine}" +
                                               $"Попробуйте повторить Ваш запрос";

        public static string VinNotFoundError = $"К сожалению не удалось найти информацию по гос. номеру.{Environment.NewLine}" +
                                           $" Попробуйте выполнить поиск по vin коду";

        public static string HelpMessage = $"Не удалось распознать запрос!{Environment.NewLine}" +
                $"Для получения информации введите гос.номер, вин код или ФИО.{Environment.NewLine}" +
                $"Примеры:{Environment.NewLine}" +
                $"XWB3K32EDCA235494{Environment.NewLine}" +
                $"Р927УТ38{Environment.NewLine}" +
                $"Иванов Иван Иванович{Environment.NewLine}";

        public static int MyUserId = 192287910;



    }
}
