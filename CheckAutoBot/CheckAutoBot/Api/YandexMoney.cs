using CheckAutoBot.Infrastructure;
using CheckAutoBot.YandexMoneyModels;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CheckAutoBot.Api
{
    public class YandexMoney
    {
        private const string Url = "https://money.yandex.ru/quickpay/confirm.xml";
        private const string Receiver = "410016722835911";
        public const string Secret = "ZeLvCCCJZCLRpxtVhR7yFhkt";

        public static string GenerateQuickpayUrl(string target, string id)
        {
            string sum = "38";
            return Url + "?" + $"receiver={Receiver.UrlEncode()}" +
                               $"&quickpay-form={"shop"}" +
                               $"&targets={target.UrlEncode()}" +
                               $"&paymentType={"AC"}" +
                               $"&sum={sum.UrlEncode()}" +
                               $"&formcomment={target.UrlEncode()}" +
                               $"&short-dest={target.UrlEncode()}" +
                               $"&label={id}";
        }

        public static Payment ConvertToPayment(string parameters)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(parameters.UrlDecode());

            var payment = new Payment();
            payment.NotificationType = nameValueCollection.Get("notification_type");
            payment.OperationId = nameValueCollection.Get("operation_id");
            payment.Amount = nameValueCollection.Get("amount");
            payment.WithdrawAmount = nameValueCollection.Get("withdraw_amount");
            payment.Currency = nameValueCollection.Get("currency");
            payment.Sender = nameValueCollection.Get("sender");
            payment.Codepro = TryParceNulable(nameValueCollection.Get("codepro"));
            payment.Label = nameValueCollection.Get("label");
            payment.Sha1Hash = nameValueCollection.Get("sha1_hash");
            payment.TestNotification = TryParceNulable(nameValueCollection.Get("test_notification"));
            payment.Unaccepted = TryParceNulable(nameValueCollection.Get("unaccepted"));
            payment.Datetime = nameValueCollection.Get("datetime");
            return payment;
        }

        public static bool TryParceNulable(string data)
        {
            bool result;
            if (bool.TryParse(data, out result))
                return result;
            return false;
        }
    }
}
