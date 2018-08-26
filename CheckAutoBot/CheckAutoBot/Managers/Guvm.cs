using CheckAutoBot.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Guvm
    {
        public CaptchaResult GetCaptcha()
        {
            var captchaUrl = "http://xn--b1afk4ade4e.xn--b1ab2a0a.xn--b1aew.xn--p1ai/services/captcha.jpg";

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "xn--b1afk4ade4e.xn--b1ab2a0a.xn--b1aew.xn--p1ai");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            headers.Add(HttpRequestHeader.Referer, "http://xn--b1afk4ade4e.xn--b1ab2a0a.xn--b1aew.xn--p1ai/info-service.htm?sid=2000");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            HttpWebRequest request = WebRequest.CreateHttp(captchaUrl);
            request.Method = "GET";
            request.Headers = headers;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string setCookie = response.Headers.Get("Set-Cookie");
            string[] elements = setCookie.Split(';');
            string value = elements[0];
            string jsessionId = value.Substring(11, value.Length - 11);

            byte[] bytes = response.ReadDataAsByteArray();
            var base64 = Convert.ToBase64String(bytes);

            return new CaptchaResult() { JsessionId = jsessionId, ImageBase64 = base64 };

        }

    }
}
