using CheckAutoBot.EaistoModels;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Eaisto
    {
        private const string url = "https://eaisto.info/";
        private const string captchaUrl = "https://eaisto.info/securimage_show.php ";


        public EaistoResult GetDiagnosticCard(string captcha,
                                      string sessionId,
                                      string vin = null,
                                      string licensePlate = null,
                                      string bodyNumber = null,
                                      string chassis = null,
                                      string eaisto = null)
        {
            var response = ExecuteRequest(captcha, sessionId, vin, licensePlate, bodyNumber, chassis, eaisto);
            return ParseHtml(response);
        }

        private string ExecuteRequest(string captcha,
                                      string sessionId,
                                      string vin = null,
                                      string licensePlate = null,
                                      string bodyNumber = null,
                                      string chassis = null,
                                      string eaisto = null)
        {
            string stringData = $"action={"checkNum"}" +
                    $"&vin={vin.UrlEncode()}" +
                    $"&registr={licensePlate.UrlEncode()}" +
                    $"&body={bodyNumber.UrlEncode()}" +
                    $"&chassis={chassis.UrlEncode()}" +
                    $"&eaisto={eaisto.UrlEncode()}" +
                    $"&captcha_code={captcha.UrlEncode()}";

            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            headers.Add(HttpRequestHeader.Host, "eaisto.info");
            headers.Add("Origin", "https://eaisto.info");
            headers.Add(HttpRequestHeader.Referer, "https://eaisto.info/");
            headers.Add("Upgrade-Insecure-Requests", "1");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            #endregion

            #region Cookie
            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie();
            cookie.Name = "PHPSESSID";
            cookie.Value = sessionId;
            cookieContainer.Add(new Uri(url), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = "POST";
            request.Headers = headers;
            request.CookieContainer = cookieContainer;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var responseData = response.ReadDataAsString();
            response.Close();

            return responseData;
        }

        public CaptchaResult GetCaptcha()
        {
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "eaisto.info");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            headers.Add(HttpRequestHeader.Referer, "https://eaisto.info/");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            HttpWebRequest request = WebRequest.CreateHttp(captchaUrl);
            request.Method = "GET";
            request.Headers = headers;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string setCookie = response.Headers.Get("Set-Cookie");
            string[] elements = setCookie.Split(';');
            string value = elements[0];
            string phpsessid = value.Substring(10, value.Length - 10);

            byte[] bytes = response.ReadDataAsByteArray();
            var base64 = Convert.ToBase64String(bytes);

            return new CaptchaResult() { SessionId = phpsessid, ImageBase64 = base64 };
        }

        private EaistoResult ParseHtml(string html)
        {
            try
            {
                string errorMessage = null;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode errorNode = doc.DocumentNode.SelectSingleNode(".//div[@id='card_alert']");

                if (!string.IsNullOrWhiteSpace(errorNode?.InnerText))
                    errorMessage = errorNode.InnerText;

                HtmlNodeCollection dcTables = doc.DocumentNode.SelectNodes(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody");

                if (dcTables == null)
                    return new EaistoResult() { ErrorMessage = errorMessage };

                var diagnosticCards = new List<DiagnosticCard>();
                foreach (var dcTable in dcTables)
                {
                    var lines = (dcTable.ChildNodes as IEnumerable<HtmlNode>).Where(x => x.Name == "tr").ToList();

                    var dictionary = lines.Select(l =>
                    {
                        var keyAndValue = l.ChildNodes.Where(chn => chn.Name == "td").ToList();
                        var key = keyAndValue[0].InnerText;
                        var value = keyAndValue[1].InnerText;

                        return new KeyValuePair<string, string>(key, value);
                    }).ToDictionary(k => k.Key, v => v.Value);

                    dictionary.TryGetValue("Марка:", out string brand);
                    dictionary.TryGetValue("Модель:", out string model);
                    dictionary.TryGetValue("Дата с:", out string dateFrom);
                    dictionary.TryGetValue("Дата до:", out string dateTo);
                    dictionary.TryGetValue("VIN:", out string vin);
                    dictionary.TryGetValue("Регистрационный номер:", out string licensePlate);
                    dictionary.TryGetValue("Номер ЕАИСТО:", out string eaistoNumber);

                    diagnosticCards.Add(new DiagnosticCard()
                    {
                        Brand = brand,
                        Model = model,
                        Vin = vin,
                        LicensePlate = licensePlate,
                        EaistoNumber = eaistoNumber,
                        DateFrom = dateFrom,
                        DateTo = dateTo
                    });
                }

                return new EaistoResult()
                {
                    DiagnosticCards = diagnosticCards,
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Eaisto HTML parsing exception", ex);
            }
        }
    }
}
