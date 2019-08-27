using CheckAutoBot.YandexMoneyModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Messages
{
    public class NewYmPaymentMessage
    {
        public Payment Payment { get; set; }

        public bool IsValid { get; set; }
    }
}
