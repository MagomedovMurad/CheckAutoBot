using CheckAutoBot.EaistoModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class EaistoParser
    {
        public EaistoResult Parse(string html)
        {
            try
            {
                string errorMessage = null;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode errorNode = doc.DocumentNode.SelectSingleNode(".//div[@id='card_alert']");

                if (!string.IsNullOrWhiteSpace(errorNode?.InnerText))
                    errorMessage = errorNode.InnerText;

                HtmlNodeCollection dcTables = doc.DocumentNode.SelectNodes(".//div[@class='col-xs-12 col-md-6 right_part']" +
                                                                           "/div[@class='table-responsive col-xs-12']" +
                                                                           "/table[@class='table table-condensed']");

                if (dcTables == null)
                    return new EaistoResult() { ErrorMessage = errorMessage };


                var currentCardNode = dcTables.FirstOrDefault(x => x.SelectNodes(".//th[@class='info']")
                                              .FirstOrDefault(y => y.InnerText == "Результаты поиска") != null);


                var historyNode = dcTables.FirstOrDefault(x => x.SelectNodes(".//th[@class='info']")
                                          .FirstOrDefault(y => y.InnerText == "История прохождения ТО") != null);

                var currentDC = TableToCurrentDc(currentCardNode);
                var DCHistory = TablesToDCHistory(historyNode);


                return new EaistoResult()
                {
                    CurrentDiagnosticCard = currentDC,
                    DiagnosticCardsHistory = DCHistory,
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Eaisto HTML parsing exception", ex);
            }
        }

        private List<DiagnosticCard> TablesToDCHistory(HtmlNode node)
        {
            if (node == null)
                return new List<DiagnosticCard>();

            var dcTables = node.SelectNodes(".//tbody/tr/td/table");
            var diagnosticCards = new List<DiagnosticCard>();
            foreach (var dcTable in dcTables)
            {
                var fromToDateNode = dcTable.SelectNodes(".//thead").FirstOrDefault();
                string[] fromToDate = fromToDateNode.InnerText.Replace("\t", "").Replace("\n", "").Split('-');
                var dateFrom = fromToDate[0];
                var dateTo = fromToDate[1];

                var lines = dcTable.SelectNodes(".//tbody/tr");

                var dictionary = lines.Select(l =>
                {
                    var keyAndValue = l.SelectNodes(".//td");  //ChildNodes.Where(chn => chn.Name == "td").ToList();
                    var key = keyAndValue[0].InnerText;
                    var value = keyAndValue[1].InnerText;

                    return new KeyValuePair<string, string>(key, value);
                }).ToDictionary(k => k.Key, v => v.Value);

                dictionary.TryGetValue("Марка:", out string brand);
                dictionary.TryGetValue("Модель:", out string model);
                //dictionary.TryGetValue("Дата с:", out string dateFrom);
                //dictionary.TryGetValue("Дата до:", out string dateTo);
                dictionary.TryGetValue("VIN:", out string vin);
                dictionary.TryGetValue("Регистрационный номер:", out string licensePlate);
                dictionary.TryGetValue("Номер ЕАИСТО:", out string eaistoNumber);
                dictionary.TryGetValue("Эксперт:", out string expert);
                dictionary.TryGetValue("Оператор:", out string @operator);

                diagnosticCards.Add(new DiagnosticCard()
                {
                    Brand = brand,
                    Model = model,
                    Vin = vin,
                    LicensePlate = licensePlate,
                    EaistoNumber = eaistoNumber,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Expert = expert,
                    Operator = @operator
                });
            }
            return diagnosticCards;
        }

        private DiagnosticCard TableToCurrentDc(HtmlNode node)
        {
            var lines = node.SelectNodes(".//tbody/tr");

            var dictionary = lines.Select(l =>
            {
                var keyAndValue = l.ChildNodes.Where(chn => chn.Name == "td").ToList();
                var key = keyAndValue[0].InnerText;
                var value = keyAndValue.ElementAtOrDefault(1)?.InnerText;

                return new KeyValuePair<string, string>(key, value);
            }).ToDictionary(k => k.Key, v => v.Value);

            dictionary.TryGetValue("Регистрационный номер:", out string licensePlate);
            dictionary.TryGetValue("VIN:", out string vin);
            dictionary.TryGetValue("Номер ЕАИСТО:", out string eaistoNumber);
            dictionary.TryGetValue("Марка:", out string brand);
            dictionary.TryGetValue("Модель:", out string model);
            dictionary.TryGetValue("Дата проведения ТО:", out string dateFrom);
            dictionary.TryGetValue("Действителен до:", out string dateTo);
            dictionary.TryGetValue("Эксперт:", out string expert);
            dictionary.TryGetValue("Оператор:", out string @operator);



            return new DiagnosticCard()
            {
                Brand = brand,
                Model = model,
                Vin = vin,
                LicensePlate = licensePlate,
                EaistoNumber = eaistoNumber,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Expert = expert,
                Operator = @operator
            };
        }
    }
}
