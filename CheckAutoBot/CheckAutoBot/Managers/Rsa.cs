//using HtmlAgilityPack;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.RsaModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CheckAutoBot.Managers
{
    public class Rsa
    {
        private readonly string _policyNotFound = "Сведения о полисе ОСАГО с указанными серией и номером не найдены";
        public Rsa()
        {
        }

        public const string policyUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm";
        public const string osagoVehicleUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/osagovehicle.htm";
        public const string dataSiteKey = "6Lf2uycUAAAAALo3u8D10FqNuSpUvUXlfP7BzHOk";

        public PolicyResponse GetPolicy(string captcha, DateTime date, string lp = "", string vin = "", string bodyNumber = "", string chassisNumber = "")
        {
            var stringData = $"vin={vin}&lp={lp.UrlEncode()}&date={date.Date.ToString("dd.MM.yyyy")}&bodyNumber={bodyNumber}&chassisNumber={chassisNumber}&captcha={captcha}";
            string json = ExecuteRequest(stringData, policyUrl);
            var policyResponse = JsonConvert.DeserializeObject<PolicyResponse>(json);

            if (!policyResponse.ValidCaptcha)
                throw new InvalidCaptchaException(captcha);

            if (policyResponse.ErrorId == 0)
            {
                if (policyResponse.WarningMessage.Equals(_policyNotFound))
                    return policyResponse;
            }
            else if (policyResponse.ErrorId == 7002 || policyResponse.ErrorId == 7005)
                return null;

            throw new Exception(policyResponse.ErrorMessage);
        }

        public VechicleResponse GetPolicyInfo(string serial, string number, DateTime date, string captcha)
        {
            string encodeSerial = WebUtility.UrlEncode(serial);
            var stringData = $"serialOsago={encodeSerial}&numberOsago={number}&dateRequest={date.Date.ToString("dd.MM.yyyy")}&captcha={captcha}";

            string json = ExecuteRequest(stringData, osagoVehicleUrl);

            var vechicleResponse = JsonConvert.DeserializeObject<VechicleResponse>(json);

            if (!vechicleResponse.ValidCaptcha)
                throw new InvalidCaptchaException(captcha);

            if (vechicleResponse.ErrorId == 0)
                return vechicleResponse;
            else if (vechicleResponse.ErrorId == 7002 || vechicleResponse.ErrorId == 7005)
                return null;

            throw new Exception($"ErrorId: {vechicleResponse?.ErrorId}. " +
                                $"Error: {vechicleResponse?.ErrorMessage}. " +
                                $"Warning: {vechicleResponse?.WarningMessage}");
        }

        private string ExecuteRequest(string stringData, string url)
        {
            var data = Encoding.ASCII.GetBytes(stringData);

            #region Headers
            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "application/json");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webRequestCollection.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            webRequestCollection.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            webRequestCollection.Add("Origin", "https://dkbm-web.autoins.ru");
            webRequestCollection.Add("Referer", url);
            webRequestCollection.Add("X-Requested-With", "XMLHttpRequest");
            webRequestCollection.Add(HttpRequestHeader.Host, "dkbm-web.autoins.ru");
            webRequestCollection.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            #endregion

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Headers = webRequestCollection;

            request.AddContent(data);

            var response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return json;
        }

        //public string GetDataSiteKey(string url)
        //{
        //    //string url = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm";
        //    string stringHtml = ExecuteRequest(url, "GET");

        //    var html = new HtmlDocument();
        //    html.LoadHtml(stringHtml);

        //    HtmlNode bodyNode = html.DocumentNode.SelectSingleNode("//div[@class='g-recaptcha']");
        //    var dataSiteKey = bodyNode.Attributes["data-sitekey"].Value;

        //    return dataSiteKey;
        //}

    }
}
