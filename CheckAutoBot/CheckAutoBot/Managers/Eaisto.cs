using CheckAutoBot.Infrastructure;
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

        public void GetDiagnosticCard(string captcha, string sessionId, string vin = null, string licensePlate = null, string bodyNumber = null, string chassis = null, string eaisto = null)
        {
            string url = $"https://www.reestr-zalogov.ru/search/endpoint";

            string stringData = $"action={"checkNum"}&vin={vin}&registr={licensePlate}&body={bodyNumber}&chassis={chassis}&eaisto={eaisto}&captcha_code={captcha}";
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
            request.Method = "POST";
            request.Headers = headers;
            request.CookieContainer = cookieContainer;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return json;
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
    }
}
