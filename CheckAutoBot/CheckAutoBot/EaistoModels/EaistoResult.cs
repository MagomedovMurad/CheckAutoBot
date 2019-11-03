using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.EaistoModels
{
    public class EaistoResult
    {
        public string ErrorMessage { get; set; }

        public DiagnosticCard CurrentDiagnosticCard { get; set; }

        public IEnumerable<DiagnosticCard> DiagnosticCardsHistory { get; set; }
    }
}
