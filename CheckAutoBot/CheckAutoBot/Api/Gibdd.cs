using Akka.Serialization;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Infrastructure.Extensions;
using Newtonsoft.Json;
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
        public const string url = "https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/history";
        public const string dataSiteKey = "6Lc66nwUAAAAANZvAnT-OK4f4D_xkdzw5MLtAYFL";

        public GibddResponse<HistoryResult> GetHistory(string vin, string captcha, string jsessionId)
        {
            var response = ExecuteRequest(vin, captcha, jsessionId, "history", "history");
            return JsonConvert.DeserializeObject<GibddResponse<HistoryResult>>(response);
        }

        public GibddResponse<DtpResult> GetDtp(string vin, string captcha, string jsessionId)
        {
            var response = ExecuteRequest(vin, captcha, jsessionId, "dtp", "aiusdtp");
            return JsonConvert.DeserializeObject<GibddResponse<DtpResult>>(response);
        }

        public GibddResponse<WantedResult> GetWanted(string vin, string captcha, string jsessionId)
        {
            var response = ExecuteRequest(vin, captcha, jsessionId, "wanted", "wanted");
            return JsonConvert.DeserializeObject<GibddResponse<WantedResult>>(response);
        }

        public GibddResponse<RestrictedResult> GetRestriction(string vin, string captcha, string jsessionId)
        {
            var response = ExecuteRequest(vin, captcha, jsessionId, "restrict", "restricted");
            return JsonConvert.DeserializeObject<GibddResponse<RestrictedResult>>(response);
        }

        public byte[] GetIncidentImage(string[] damagePoints)
        {
            var accidentImageUrl = GetAccidentImageLink(damagePoints);

            HttpWebRequest request = WebRequest.CreateHttp(accidentImageUrl); //xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/dtp/damages.png?map=
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var photoBinaryData = response.ReadDataAsByteArray();
            response.Close();

            return photoBinaryData;
        }

        public string GetAccidentImageLink(string[] damagePoints)
        {
            var strDamagePoints = string.Join("", damagePoints);
            return $"http://check.gibdd.ru/proxy/check/auto/images/cache/{strDamagePoints}.png";
        }

        private string ExecuteRequest(string vin, string reCaptchaToken, string jsessionId, string path, string checkType)
        {
            string url = $"https://xn--b1afk4ade.xn--90adear.xn--p1ai/proxy/check/auto/{path}";
            string stringData = $"vin={vin}&captchaWord={null}&checkType={checkType}&reCaptchaToken={reCaptchaToken}";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "xn--b1afk4ade.xn--90adear.xn--p1ai");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            headers.Add("Origin", "https://xn--90adear.xn--p1ai");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
            headers.Add(HttpRequestHeader.Referer, "https://xn--90adear.xn--p1ai/check/auto");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add("Sec-Fetch-Mode", "cors");
            headers.Add("Sec-Fetch-Site", "same-site");
            #endregion

            #region Cookie
            //CookieContainer cookieContainer = new CookieContainer();
            //Cookie cookie = new Cookie();
            //cookie.Name = "JSESSIONID";
            //cookie.Value = jsessionId;
            //cookieContainer.Add(new Uri(url), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.Headers = headers;
            //request.CookieContainer = cookieContainer;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
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

            byte[] bytes = response.ReadDataAsByteArray();
            var base64 = Convert.ToBase64String(bytes);

            return new CaptchaResult() { SessionId = jsessionId, ImageBase64 = base64 };
        }
    }

    public class CaptchaResult
    {
        public string ImageBase64 { get; set; }

        public string SessionId { get; set; }
             
    }
        
}
