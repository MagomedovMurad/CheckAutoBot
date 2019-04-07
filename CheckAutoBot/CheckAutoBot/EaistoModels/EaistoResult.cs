using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.EaistoModels
{
    public class EaistoResult
    {
        public string ErrorMessage { get; set; }

        public IEnumerable<DiagnosticCard> DiagnosticCards { get; set; }
    }
}
