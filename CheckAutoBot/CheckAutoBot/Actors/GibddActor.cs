using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class GibddActor: ReceiveActor
    {
        HttpClient _httpClient;
        HttpClientHandler _handler;

        public GibddActor()
        {
            _handler = new HttpClientHandler();
            _httpClient = new HttpClient(_handler);
        }

        public string GetHistory(string vin, string captcha, string jsessionId)
        {
            string historyUrl = "https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/history";

            string stringData = $"vin={vin}&captchaWord={captcha}&checkType=history";
            byte[] data = Encoding.ASCII.GetBytes(stringData);


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, historyUrl);
            request.Headers.Add("Host", "xn--b1afk4ade.xn--90adear.xn--p1ai");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("Origin", "https://xn--90adear.xn--p1ai");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            request.Headers.Add("Referer", "https://xn--90adear.xn--p1ai/check/auto/");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru,en;q=0.9");
            request.Headers.Add("Cookie", $"JSESSIONID={jsessionId}");

            HttpContent requestContent = new ByteArrayContent(data, 0, data.Length);
            requestContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.Content = requestContent;

            var cookieContainer = new CookieContainer();
            var cookie = new Cookie();
            cookie.Name = "JSESSIONID";
            cookie.Value = jsessionId;
            cookieContainer.Add(new Uri(historyUrl), cookie);

            _handler = new HttpClientHandler();
            _handler.CookieContainer = cookieContainer;

            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            var httpContent = response.Content;

            var responseContent = httpContent.ReadAsStringAsync().Result;

            return responseContent;
        }


        public CaptchaResult GetCaptha()
        {
            var date = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            string captchaUrl = "http://сервис.гибдд.рф/proxy/captcha.jpg?" + Math.Round(date, 0);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, captchaUrl);
            request.Headers.Add("Host", "xn--b1afk4ade.xn--90adear.xn--p1ai");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            request.Headers.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
            request.Headers.Add("Referer", "https://xn--90adear.xn--p1ai/check/auto");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            IEnumerable<string> headerValues = new List<string>();

            bool jsessionIdIsExist = response.Headers.TryGetValues("Set-Cookie", out headerValues);

            var headerValue = headerValues.ElementAt(0);
            var jsessionId = headerValue.Split(';')[0];
            var jsessionIdValue = jsessionId.Substring(11, jsessionId.Length - 11);

            var httpContent = response.Content;
            byte[] content = httpContent.ReadAsByteArrayAsync().Result;
            var base64 = Convert.ToBase64String(content);

            return new CaptchaResult() { ImageBase64 = base64, JsessionId = jsessionIdValue };
        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string JsessionId { get; set; }

    }
}
