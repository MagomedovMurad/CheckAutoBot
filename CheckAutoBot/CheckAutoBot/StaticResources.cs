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
    }
}
