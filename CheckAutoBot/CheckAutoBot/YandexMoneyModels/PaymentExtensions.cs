using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CheckAutoBot.YandexMoneyModels
{
    public static class PaymentExtensions
    {
        public static bool IsValid(this Payment payment, string secret)
        {
            var parameters = $"{payment.NotificationType}&" +
                       $"{payment.OperationId}&" +
                       $"{payment.Amount}&" +
                       $"{payment.Currency}&" +
                       $"{payment.Datetime}&" +
                       $"{payment.Sender}&" +
                       $"{payment.Codepro.ToString().ToLower()}&" +
                       $"{secret}&" +
                       $"{payment.Label}";

            var hash = GetSha1(parameters);
           
            return hash == payment.Sha1Hash;
        }

        public static string GetSha1(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new SHA1Managed().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("x2");
            }
            return hash;
        }
    }
}
