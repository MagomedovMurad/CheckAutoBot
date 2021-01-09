using CheckAutoBot.Exceptions;
using CheckAutoBot.FnpModels;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Fnp
    {
        public const string url = "https://www.reestr-zalogov.ru/search/index";
        public const string googleKey = "6LdKJhMaAAAAAIfeHC6FZc-UVfzDQpiOjaJUWoxr";
        public PledgeResponse GetPledges(string vin, string captcha, string jsessionId)
        {
            string url = $"https://www.reestr-zalogov.ru/api/search/notary?token={captcha}";

            //string stringData = $"VIN={vin}&formName=vehicle-form&token={captcha}&uuid={Guid.NewGuid()}";

            var filter = JsonConvert.SerializeObject(GenerateFilter(vin));
            byte[] content = Encoding.ASCII.GetBytes(filter);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "www.reestr-zalogov.ru");
            headers.Add(HttpRequestHeader.ContentLength, content.Length.ToString());
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add("Origin", "https://www.reestr-zalogov.ru");
            headers.Add("X-Requested-With", "XMLHttpRequest");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            headers.Add(HttpRequestHeader.ContentType, "application/json;charset=UTF-8");
            headers.Add(HttpRequestHeader.Referer, "https://www.reestr-zalogov.ru/search/index");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            #endregion

            #region Cookie
            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie();
            cookie.Name = "JSESSIONID";
            cookie.Value = "B4E1A1EFFA59997B1D32200414979D79";
            cookieContainer.Add(new Uri(url), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.Headers = headers;
            request.CookieContainer = cookieContainer;

            request.AddContent(content);
            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return JsonConvert.DeserializeObject<PledgeResponse>(json);
        }

        private SearchFilter GenerateFilter(string vin)
        {
            var vechicleProperty = new VechicleProperty()
            {
                Vin = vin
            };

            var filterProperty = new FilterProperty()
            {
                VechicleProperty = vechicleProperty
            };

            var filter = new Filter()
            {
                Property = filterProperty
            };

            return new SearchFilter()
            {
                Mode = SearchMode.All,
                Filter = filter,
                Limit = 50,
                Offset = 0
            };
        }

        //public CaptchaResult GetCaptcha()
        //{
        //    Random random = new Random();
        //    var randomValue = random.NextDouble() * 10000;

        //    var captchaUrl = "https://www.reestr-zalogov.ru/captcha/generate?" + randomValue;

        //    WebHeaderCollection headers = new WebHeaderCollection();
        //    headers.Add(HttpRequestHeader.Host, "www.reestr-zalogov.ru");
        //    headers.Add(HttpRequestHeader.Connection, "keep-alive");
        //    headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
        //    headers.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
        //    headers.Add(HttpRequestHeader.Referer, "https://www.reestr-zalogov.ru/search/index");
        //    headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
        //    headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

        //    HttpWebRequest request = WebRequest.CreateHttp(captchaUrl);
        //    request.Method = "GET";
        //    request.Headers = headers;

        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //    string setCookie = response.Headers.Get("Set-Cookie");
        //    string[] elements = setCookie.Split(';');
        //    string value = elements[0];
        //    string jsessionId = value.Substring(11, value.Length - 11);

        //    byte[] bytes = response.ReadDataAsByteArray();
        //    var base64 = Convert.ToBase64String(bytes);

        //    return new CaptchaResult() { SessionId = jsessionId, ImageBase64 = base64 };
        //}

    }
}
