using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models.DataSource
{
    public class DiagnosticCardsHistory
    {
        public DiagnosticCardsHistory()
        {
        }
        public DiagnosticCardsHistory(IEnumerable<DiagnosticCard> diagnosticCards)
        {
            DiagnosticCards = diagnosticCards;
        }

        public IEnumerable<DiagnosticCard> DiagnosticCards { get; set; }
    }
}
