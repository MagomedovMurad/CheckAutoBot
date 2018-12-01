using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Api
{
    public class YandexMoney
    {
        private const string Url = "https://money.yandex.ru/quickpay/confirm.xml";
        private const string Receiver = "";

        public string GenerateQuickpayUrl(string target, string sum, string id)
        {
            return Url + "?" + $"receiver={Receiver}" +
                               $"quickpay-form={"shop"}" +
                               $"targets={target}" +
                               $"paymentType={"AC"}" +
                               $"sum={sum}" +
                               $"formcomment={target}" +
                               $"short-dest={target}" +
                               $"label={id}";
        }
    }
}
