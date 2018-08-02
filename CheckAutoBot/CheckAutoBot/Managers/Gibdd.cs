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
        public void GetHistory(string vin, string captcha, string jsessionId)
        {
            string historyUrl = "https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/history";

            string stringData = $"vin={vin}&captchaWord={captcha}&checkType=history";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(historyUrl);
            request.Method = "POST";
            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webRequestCollection.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            webRequestCollection.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            webRequestCollection.Add("Origin", "https://xn--90adear.xn--p1ai");
            webRequestCollection.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            webRequestCollection.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            webRequestCollection.Add("Referer", "https://xn--90adear.xn--p1ai/check/auto/");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru,en;q=0.9");
            webRequestCollection.Add(HttpRequestHeader.Cookie, $"JSESSIONID={jsessionId}");

            var container = new CookieContainer();

            request.Headers = webRequestCollection;

            // cookies.ForEach(x => container.Add(x));

            var cookie1 = new Cookie();
            cookie1.Name = "JSESSIONID";
            cookie1.Value = jsessionId;

            container.Add(new Uri(historyUrl), cookie1);

            request.CookieContainer = container;


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

        //public void TestMethod()
        //{
        //    var webRequestCollection = new WebHeaderCollection();
        //    webRequestCollection.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
        //    webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
        //    webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        //    webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");

        //    webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
        //    webRequestCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
        //    webRequestCollection.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

        //    WebClient c = new WebClient();
        //    c.Headers = webRequestCollection;
        //    byte[] responseData = c.DownloadData(captchaUrl);
        //    var tr = Convert.ToBase64String(responseData);
        //    var th = c.ResponseHeaders;
        //    var r = th.Get("Set-Cookie"); //35493B526CCFED4B8578BCE3593F02B2
        //}

        public CaptchaResult GetCaptha()
        {
            var date = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string captchaUrl = "http://сервис.гибдд.рф/proxy/captcha.jpg?" + Math.Round(date, 0);

        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(captchaUrl);
         //   request.Method = "GET";

            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webRequestCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            webRequestCollection.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            
            //request.Headers = webRequestCollection;


            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            HttpClient httpClient = new HttpClient(handler);
            HttpResponseMessage httpClientResponse = httpClient.GetAsync(new Uri(captchaUrl)).Result;
            var headerValues = httpClientResponse.Headers.GetValues("Set-Cookie");
            var headerValue = headerValues.ElementAt(0);
            var header = headerValue.Split(';')[0];
            var jsessionId = header.Substring(11, header.Length - 11);

            Uri uri = new Uri(captchaUrl);
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            //foreach (Cookie cookie in responseCookies)
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);




            var httpContent = httpClientResponse.Content;
            byte[] content = httpContent.ReadAsByteArrayAsync().Result;
            var base64 = Convert.ToBase64String(content);


            File.WriteAllBytes(@"C:\Users\m.magomedov\Chaptcha.png", content);

                //WebClient client = new WebClient();

                //client.Headers = webRequestCollection;
                //byte[] responseData = client.DownloadData(captchaUrl);
                //var tr = Convert.ToBase64String(responseData);
                //var th = client.ResponseHeaders;
                //var r = th.Get("Set-Cookie"); //35493B526CCFED4B8578BCE3593F02B2

                return new CaptchaResult() { ImageBase64 = base64, SessionId = jsessionId, Cookies = responseCookies };

        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string SessionId { get; set; }

        public IEnumerable<Cookie> Cookies { get; set; }
             
    }
        
}
