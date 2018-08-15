using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Fnp
    {


        private CaptchaResult GetCaptcha()
        {
            Random random = new Random();
            var randomValue = random.NextDouble() * 10000;

            var url = "https://www.reestr-zalogov.ru/captcha/generate?" + randomValue;


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
    }
}
