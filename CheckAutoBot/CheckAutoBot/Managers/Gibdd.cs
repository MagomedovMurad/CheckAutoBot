using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Gibdd
    {
        public void GetHistory(string captcha, string sessionId)
        {
            string historyUrl = "https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/history";

            string stringData = $"vin=X9FMXXEEBMCB65023&captchaWord={captcha}&checkType=history";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(historyUrl);
            request.Method = "POST";
            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru,en;q=0.9");
            webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webRequestCollection.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            webRequestCollection.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            webRequestCollection.Add("DNT", "1");
            webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webRequestCollection.Add("Origin", "https://xn--90adear.xn--p1ai");
            webRequestCollection.Add("Referer", "https://xn--90adear.xn--p1ai/check/auto/");
            webRequestCollection.Add(HttpRequestHeader.Cookie, $"JSESSIONID={sessionId}");
            webRequestCollection.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");

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

        public void TestMethod()
        {
            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");

            webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webRequestCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            webRequestCollection.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

            WebClient c = new WebClient();
            c.Headers = webRequestCollection;
            byte[] responseData = c.DownloadData(captchaUrl);
            var tr = Convert.ToBase64String(responseData);
            var th = c.ResponseHeaders;
            var r = th.Get("Set-Cookie"); //35493B526CCFED4B8578BCE3593F02B2
        }

        public CaptchaResult GetCaptha()
        {
            var date = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string captchaUrl = "http://сервис.гибдд.рф/proxy/captcha.jpg?" + Math.Round(date, 0);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(captchaUrl);
            request.Method = "GET";

            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
          //  webRequestCollection.Add("Connection", "keep-alive");
            //webRequestCollection.Add(HttpRequestHeader.Cookie,"JSESSIONID=2938EC1ADF4FD794FF24E8FED09B6F30");
            webRequestCollection.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            webRequestCollection.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            webRequestCollection.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            
            request.Headers = webRequestCollection;


            WebClient client = new WebClient();
            
            client.Headers = webRequestCollection;
            byte[] responseData = client.DownloadData(captchaUrl);
            var tr = Convert.ToBase64String(responseData);
            var th = client.ResponseHeaders;
            var r = th.Get("Set-Cookie"); //35493B526CCFED4B8578BCE3593F02B2

            return new CaptchaResult() { ImageBase64 = tr, SessionId = r.Substring(11, 32)};

            var response = request.GetResponse();
            var td = response.Headers;
            string stringSource;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {

                    stringSource = reader.ReadToEnd();
                    var bytes = Encoding.Default.GetBytes(stringSource);

                    var t = Convert.ToBase64String(bytes);
                }
            }
            response.Close();
          // return stringSource;

            // byte[] bitmap = Encoding.ASCII.GetBytes(stringSource);

            //Convert.to


        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string SessionId { get; set; }
             
    }
        
}
