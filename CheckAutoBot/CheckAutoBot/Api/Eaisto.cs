using CheckAutoBot.EaistoModels;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Infrastructure.Extensions;
using CheckAutoBot.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Eaisto
    {
        public const string url = "https://eaisto.info/";
        private const string url2 = "https://eaisto.info/osago/getdata.php";
        public const string dataSiteKeyV2 = "6LcssDwUAAAAAHAw_cJQHuNgmf4k4m9pdX-p1Mu5";
        public const string dataSiteKeyV3 = "6LemMrYUAAAAAEgj7AVh1Cy-av2zYJahbgqBYISZ";

        public EaistoResult GetDiagnosticCards(string captchaV2, string captchaV3, string vin = null, string registr = null, string body = null, string chassis = null, string eaisto = null)
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Host", "eaisto.info");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Cache-Control", "max-age=0");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.AddHeader("Origin", "https://eaisto.info");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Referer", "https://eaisto.info/");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("Sec-Fetch-Site", "same-origin");
            request.AddHeader("Sec-Fetch-Mode", "navigate");
            request.AddHeader("Sec-Fetch-Dest", "document");

            request.AddParameter("action", "checkNum");
            request.AddParameter("ch_par", 5);
            request.AddParameter("registr", registr);
            request.AddParameter("vin", vin);
            request.AddParameter("body", body);
            request.AddParameter("chassis", chassis);
            request.AddParameter("eaisto", eaisto);
            request.AddParameter("g-recaptcha-response", captchaV3);
            request.AddParameter("captchaResp", captchaV2);


            RestClient restClient = new RestClient("https://eaisto.info/");
            restClient.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36";
            IRestResponse response = restClient.Execute(request);
            return new EaistoParser().Parse(response.Content);
           // return response.Content;
        }


        //public EaistoResult GetDiagnosticCards(string captcha,
        //                              string sessionId,
        //                              string vin = null,
        //                              string licensePlate = null,
        //                              string bodyNumber = null,
        //                              string chassis = null,
        //                              string eaisto = null)
        //{
        //    var response = ExecuteRequest(captcha, sessionId, vin, licensePlate, bodyNumber, chassis, eaisto);
        //    return ParseHtml(response);
        //}

        public DiagnosticCard GetLastDiagnosticCard(string licensePlate)
        {
            var response = ExecuteRequest(licensePlate, null);
            return JsonConvert.DeserializeObject<DiagnosticCard>(response);
        }

        private string ExecuteRequest(string licensePlate, string sessionId)
        {
            var stringData = $"action=getDataByLicensePlate&licensePlate={licensePlate.UrlEncode()}";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "eaisto.info");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add("Origin", "https://eaisto.info");
            headers.Add("X-Requested-With", "XMLHttpRequest");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36");
            headers.Add("Sec-Fetch-Mode", "cors");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add("Sec-Fetch-Site", "same-origin");
            headers.Add(HttpRequestHeader.Referer, "https://eaisto.info/osago/");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            #endregion

            #region Cookie
            CookieContainer cookieContainer = new CookieContainer();
            Cookie cookie = new Cookie();
            cookie.Name = "PHPSESSID";
            cookie.Value = sessionId;
            cookieContainer.Add(new Uri(url2), cookie);
            #endregion

            HttpWebRequest request = WebRequest.CreateHttp(url2);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = "POST";
            request.Headers = headers;
            request.CookieContainer = cookieContainer;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var responseData = response.ReadDataAsString();
            response.Close();

            return responseData;
        }

        //private string ExecuteRequest(string captcha,
        //                              string sessionId,
        //                              string vin = null,
        //                              string licensePlate = null,
        //                              string bodyNumber = null,
        //                              string chassis = null,
        //                              string eaisto = null)
        //{
        //    string stringData = $"action={"checkNum"}" +
        //            $"&vin={vin.UrlEncode()}" +
        //            $"&registr={licensePlate.UrlEncode()}" +
        //            $"&body={bodyNumber.UrlEncode()}" +
        //            $"&chassis={chassis.UrlEncode()}" +
        //            $"&eaisto={eaisto.UrlEncode()}" +
        //            $"&captcha_code={captcha.UrlEncode()}";

        //    byte[] data = Encoding.ASCII.GetBytes(stringData);

        //    #region Headers
        //    WebHeaderCollection headers = new WebHeaderCollection();
        //    headers.Add(HttpRequestHeader.Host, "eaisto.info");
        //    headers.Add(HttpRequestHeader.Connection, "keep-alive");
        //    headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
        //    headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
        //    headers.Add("Origin", "https://eaisto.info");
        //    headers.Add("Upgrade-Insecure-Requests", "1");
        //    headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
        //    headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36");
        //    headers.Add("Sec-Fetch-Mode", "navigate");
        //    headers.Add("Sec-Fetch-User", "?1");
        //    headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
        //    headers.Add("Sec-Fetch-Site", "same-origin");
        //    headers.Add(HttpRequestHeader.Referer, "https://eaisto.info/");
        //    headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
        //    headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        //    #endregion

        //    #region Cookie
        //    CookieContainer cookieContainer = new CookieContainer();
        //    Cookie cookie = new Cookie();
        //    cookie.Name = "PHPSESSID";
        //    cookie.Value = sessionId;
        //    cookieContainer.Add(new Uri(url), cookie);
        //    #endregion

        //    HttpWebRequest request = WebRequest.CreateHttp(url);
        //    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        //    request.Method = "POST";
        //    request.Headers = headers;
        //    request.CookieContainer = cookieContainer;

        //    request.AddContent(data);

        //    WebResponse response = request.GetResponse();
        //    var responseData = response.ReadDataAsString();
        //    response.Close();

        //    return responseData;
        //}
    }
}
