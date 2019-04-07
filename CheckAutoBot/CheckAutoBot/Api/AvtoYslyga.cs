using CheckAutoBot.AvtoYslygaModels;
using CheckAutoBot.Infrastructure;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CheckAutoBot.Api
{
    public class AvtoYslyga
    {
        private const string Url = "https://avto-yslyga.ru/wp-content/themes/auto/template-parts/check-inspection-handler.php";
        public const string DataSiteKey = "6LeBPAcUAAAAAF060gcWCfNEjO4vQAAbKOyIyGTR";
        public const string Key = "5ae032b75566215ba300b886e3741db6";

        private AvtoYslygaResult GetDiagnosticCard(string captcha,
                                      string licensePlate)
        {
            var response = ExecuteRequest(captcha, licensePlate);
            return ParseHtml(response);
        }

        private AvtoYslygaResult ParseHtml(string html)
        {
            try
            {
                if(!html.StartsWith("<div"))
                    return new AvtoYslygaResult() { ErrorMessage = html };


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNodeCollection dcTables = doc.DocumentNode.SelectNodes("//div[@class='right']/ol/li");

                //var lastDcTable = dcTables[0];
                var diagnosticCards = new List<DiagnosticCard>();
                foreach (var dcTable in dcTables)
                {
                    var lines = (dcTable.ChildNodes as IEnumerable<HtmlNode>).Where(x => x.Name == "p").ToList();

                    var dictionary = lines.Select(l =>
                    {
                        var key = l.ChildNodes.FirstOrDefault(x => x.Name == "span").InnerText;
                        var value = l.ChildNodes.FirstOrDefault(x => x.Name == "b").InnerText;

                        return new KeyValuePair<string, string>(key, value);
                    }).ToDictionary(k => k.Key, v => v.Value);

                    dictionary.TryGetValue("Номер ДК:", out string eaistoNumber);
                    dictionary.TryGetValue("Марка/модель:", out string brandAndModel);
                    dictionary.TryGetValue("VIN ТС:", out string vin);
                    dictionary.TryGetValue("Номер кузова ТС:", out string frameNumber);
                    dictionary.TryGetValue("Гос.номер ТС:", out string licensePlate);
                    dictionary.TryGetValue("Дата диагностики:", out string dateFrom);
                    dictionary.TryGetValue("Срок действия до:", out string dateTo);
                    dictionary.TryGetValue("Оператор:", out string operatorName);

                    diagnosticCards.Add(new DiagnosticCard()
                    {
                        EaistoNumber = eaistoNumber,
                        BrandAndModel = brandAndModel,
                        Vin = vin,
                        FrameNumber = frameNumber,
                        LicensePlate = licensePlate,
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        OperatorName = operatorName
                    });
                }

                return new AvtoYslygaResult()
                {
                    DiagnosticCards = diagnosticCards,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Eaisto HTML parsing exception", ex);
            }
        }

        private string ExecuteRequest(string reCaptchaToken,
                                      string licensePlate)
        {
            string stringData = $"regNumber={licensePlate}&g-recaptcha-response={reCaptchaToken}&key={Key}";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add("Origin", "https://avto-yslyga.ru");
            headers.Add(HttpRequestHeader.Referer, "https://avto-yslyga.ru/proverit-tekhosmotr/");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            #endregion

            #region Cookie
            //CookieContainer cookieContainer = new CookieContainer();
            //Cookie cookie = new Cookie();
            //cookie.Name = "JSESSIONID";
            //cookie.Value = jsessionId;
            //cookieContainer.Add(new Uri(url), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(Url);
            request.Method = "POST";
            request.Headers = headers;
            //request.CookieContainer = cookieContainer;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return json;
        }
    }
}
