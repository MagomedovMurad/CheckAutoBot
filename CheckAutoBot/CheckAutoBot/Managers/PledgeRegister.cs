﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class PledgeRegister
    {
        public void GetPledges(string vin, string captcha, string jsessionId)
        {
            //Host: www.reestr-zalogov.ru
            //Connection: keep-alive
            //Content-Length: 97
            //Accept: */*
            //Origin: https://www.reestr-zalogov.ru
            //X-Requested-With: XMLHttpRequest
            //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36
            //Content-Type: application/x-www-form-urlencoded; charset=UTF-8
            //Referer: https://www.reestr-zalogov.ru/search/index
            //Accept-Encoding: gzip, deflate, br
            //Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7
            //Cookie: JSESSIONID=8800436C4DDC85CDDEB50FC5D280B015



            string url = $"https://www.reestr-zalogov.ru/search/endpoint";
            string stringData = $"VIN={vin}&formName=vehicle-form&token=wynne&uuid=2b7fd9c9-eb86-4837-8250-6312c296be74";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "www.reestr-zalogov.ru");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add("Origin", "https://www.reestr-zalogov.ru");
            headers.Add("X-Requested-With", "XMLHttpRequest");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 YaBrowser/18.6.1.770 Yowser/2.5 Safari/537.36");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add(HttpRequestHeader.Referer, "https://www.reestr-zalogov.ru/search/index");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
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

        private CaptchaResult GetCaptcha()
        {
            Random random = new Random();
            var randomValue = random.NextDouble() * 10000;

            var captchaUrl = "https://www.reestr-zalogov.ru/captcha/generate?" + randomValue;

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "www.reestr-zalogov.ru");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add(HttpRequestHeader.Accept, "image/webp,image/apng,image/*,*/*;q=0.8");
            headers.Add(HttpRequestHeader.Referer, "https://www.reestr-zalogov.ru/search/index");
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

    }
}
