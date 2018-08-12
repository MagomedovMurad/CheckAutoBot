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
        public Gibdd()
        {
        }

        public string GetHistory(string vin, string captcha, string jsessionId)
        {
            return ExecuteRequest(vin, captcha, jsessionId, "history", "history");
        }

        public string GetDtp(string vin, string captcha, string jsessionId)
        {
            return ExecuteRequest(vin, captcha, jsessionId, "dtp", "aiusdtp");
        }

        public string GetWanted(string vin, string captcha, string jsessionId)
        {
            return ExecuteRequest(vin, captcha, jsessionId, "wanted", "wanted");
        }

        public string GetRestriction(string vin, string captcha, string jsessionId)
        {
            return ExecuteRequest(vin, captcha, jsessionId, "restrict", "restricted");
        }

        private string ExecuteRequest(string vin, string captcha, string jsessionId, string path, string checkType)
        {
            string url = $"https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/{path}";
            string stringData = $"vin={vin}&captchaWord={captcha}&checkType={checkType}";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            headers.Add("Origin", "https://xn--90adear.xn--p1ai");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            headers.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto/");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru,en;q=0.9");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            #endregion

            #region Cookie
            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie();
            cookie.Name = "JSESSIONID";
            cookie.Value = jsessionId;
            cookieContainer.Add(new Uri(url), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.Headers = headers;
            request.CookieContainer = cookieContainer;

            AddRequestContent(request, data);

            WebResponse response = request.GetResponse();
            var json = ResponseToString(response);
            response.Close();

            return json;
        }

        public CaptchaResult GetCaptcha()
        {
            var date = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string captchaUrl = "http://сервис.гибдд.рф/proxy/captcha.jpg?" + Math.Round(date, 0);

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            headers.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            HttpWebRequest request = WebRequest.CreateHttp(captchaUrl);
            request.Method = "GET";
            request.Headers = headers;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string setCookie = response.Headers.Get("Set-Cookie");
            string[] elements = setCookie.Split(';');
            string value = elements[0];
            string jsessionId = value.Substring(11, value.Length - 11);

            byte[] bytes = ResponseToByteArray(response);
            var base64 = Convert.ToBase64String(bytes);

            return new CaptchaResult() { JsessionId = jsessionId, ImageBase64 = base64 };
        }

        private byte[] ResponseToByteArray(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private string ResponseToString(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        private void AddRequestContent(HttpWebRequest request, byte[] data)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string JsessionId { get; set; }
             
    }
        
}
