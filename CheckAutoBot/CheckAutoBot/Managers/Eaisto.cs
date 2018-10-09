using CheckAutoBot.EaistoModels;
using CheckAutoBot.Infrastructure;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Eaisto
    {
        private const string url = "https://eaisto.info/";
        private const string captchaUrl = "https://eaisto.info/securimage_show.php ";

        public DiagnosticCard GetDiagnosticCard(string captcha,
                                      string phoneNumber,
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
                                $"&phoneNumber={phoneNumber.UrlEncode()}" +
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

            return ParseHtml(responseData);
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

        private DiagnosticCard ParseHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode brandNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[1]/td[2]");

            HtmlNode modelNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[2]/td[2]");

            HtmlNode dateFromeNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[3]/td[2]");

            HtmlNode dateToNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[4]/td[2]");

            HtmlNode vinNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[5]/td[2]");

            HtmlNode licensePlateNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[6]/td[2]");

            HtmlNode eaistoNumberNode = doc.DocumentNode.SelectSingleNode(".//div[@class='col-xs-12 col-md-6 right_part']/div/table/tbody/tr[7]/td[2]");

            return new DiagnosticCard()
            {
                Brand = brandNode?.InnerText,
                Model = modelNode?.InnerText,
                Vin = vinNode?.InnerText,
                LicensePlate = vinNode?.InnerText,
                EaistoNumber = eaistoNumberNode?.InnerText,
                DateFrom = dateFromeNode?.InnerText,
                DateTo = dateToNode?.InnerText
            };
        }
    }
}
