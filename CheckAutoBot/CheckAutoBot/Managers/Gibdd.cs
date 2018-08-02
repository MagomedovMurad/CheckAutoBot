using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Gibdd
    {
        HttpClient _httpClient;
        HttpClientHandler _handler;


        public Gibdd()
        {
            _handler = new HttpClientHandler();
            _httpClient = new HttpClient(_handler);
        }

        public void GetHistory(string vin, string captcha, string jsessionId)
        {
            string historyUrl = "https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/history";

            string stringData = $"vin={vin}&captchaWord={captcha}&checkType=history";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(historyUrl);
            request.Method = "POST";
            var webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webHeaderCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webHeaderCollection.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            webHeaderCollection.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            webHeaderCollection.Add("Origin", "https://xn--90adear.xn--p1ai");
            webHeaderCollection.Add(HttpRequestHeader.UserAgent, 
                                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            webHeaderCollection.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            webHeaderCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto/");
            webHeaderCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webHeaderCollection.Add(HttpRequestHeader.AcceptLanguage, "ru,en;q=0.9");
            webHeaderCollection.Add(HttpRequestHeader.Cookie, $"JSESSIONID={jsessionId}");

            var cookieContainer = new CookieContainer();

            request.Headers = webHeaderCollection;

            var cookie = new Cookie();
            cookie.Name = "JSESSIONID";
            cookie.Value = jsessionId;

            cookieContainer.Add(new Uri(historyUrl), cookie);

            request.CookieContainer = cookieContainer;


            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = request.GetResponse();
            string source;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    source = reader.ReadToEnd();
                }
            }
            response.Close();
        }

        public void GetDtp()
        {
            string dtpUrl = "";

            string stringData = "";

        }

        public void GetWanted()
        {

        }

        public void GetRestriction()
        {

        }

        public CaptchaResult GetCaptha()
        {
            var date = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string captchaUrl = "http://сервис.гибдд.рф/proxy/captcha.jpg?" + Math.Round(date, 0);

            var webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webHeaderCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webHeaderCollection.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            webHeaderCollection.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            webHeaderCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            webHeaderCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webHeaderCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");


            CookieContainer cookies = new CookieContainer();

            //_handler.CookieContainer = cookies;

            HttpResponseMessage httpClientResponse = _httpClient.GetAsync(new Uri(captchaUrl)).Result;
            var headerValues = httpClientResponse.Headers.GetValues("Set-Cookie");
            var headerValue = headerValues.ElementAt(0);
            var header = headerValue.Split(';')[0];
            var jsessionId = header.Substring(11, header.Length - 11);

            Uri uri = new Uri(captchaUrl);

            var httpContent = httpClientResponse.Content;
            byte[] content = httpContent.ReadAsByteArrayAsync().Result;
            var base64 = Convert.ToBase64String(content);


            //File.WriteAllBytes(@"C:\Users\m.magomedov\Chaptcha.png", content);

            return new CaptchaResult() { ImageBase64 = base64, SessionId = jsessionId };

        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string SessionId { get; set; }
             
    }
        
}
