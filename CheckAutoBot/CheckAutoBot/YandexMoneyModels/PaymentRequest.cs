using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.YandexMoneyModels
{
    public class PaymentRequest
    {
        public Payment Payment { get; set; }

        public bool IsValid { get; set; }
    }
}
