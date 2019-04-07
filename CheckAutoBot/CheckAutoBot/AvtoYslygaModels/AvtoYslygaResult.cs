using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.AvtoYslygaModels
{
    public class AvtoYslygaResult
    {
        public string ErrorMessage { get; set; }

        public IEnumerable<DiagnosticCard> DiagnosticCards { get; set; }
    }
}
