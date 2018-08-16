﻿//using HtmlAgilityPack;
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
        public Rsa()
        {
        }

        public const string policyUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm";
        public const string osagoVehicleUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/osagovehicle.htm";
        public const string dataSiteKey = "6Lf2uycUAAAAALo3u8D10FqNuSpUvUXlfP7BzHOk";

        public PolicyResponse GetPolicy(string captcha, DateTime date, string lp = "", string vin = "", string bodyNumber = "", string chassisNumber = "")
        {
            string encodeLp = WebUtility.UrlEncode(lp);
            var stringData = $"vin={vin}&lp={encodeLp}&date={date.Date.ToString("dd.MM.yyyy")}&bodyNumber={bodyNumber}&chassisNumber={chassisNumber}&captcha={captcha}";
            //string json = ExecuteRequest(stringData, policyUrl);

            string json = "{\"bodyNumber\":\"\",\"chassisNumber\":\"\",\"licensePlate\":\"Р928УТ26\",\"vin\":\"\",\"policyUnqId\":null,	\"policyResponseUIItems\":[{\"insCompanyName\":\"ИНГОССТРАХ\",\"policyBsoNumber\":\"0031852791\",\"policyBsoSerial\":\"ХХХ\",\"policyIsRestrict\":\"1\",\"policyUnqId\":\"324402046\"}],\"validCaptcha\":true,\"errorMessage\":null,\"errorId\":0,\"warningMessage\":null}";
            var data = JsonConvert.DeserializeObject<PolicyResponse>(json);
            return data;
        }

        public VechicleResponse GetPolicyInfo(string serial, string number, DateTime date, string captcha)
        {
            string encodeSerial = WebUtility.UrlEncode(serial);
            var stringData = $"serialOsago={encodeSerial}&numberOsago={number}&dateRequest={date.Date.ToString("dd.MM.yyyy")}&captcha={captcha}";

            //string json = ExecuteRequest(stringData, osagoVehicleUrl);

            string json = "{\"bodyNumber\":null,\"chassisNumber\":null,\"licensePlate\":\"P928YT26RUS\",\"insurerName\":\"ИНГОССТРАХ\",\"policyStatus\":\"Действует\",\"vin\":\"XTA21150064291647\",\"validCaptcha\":true,\"errorMessage\":null,\"errorId\":0,\"warningMessage\":null}";

            var data = JsonConvert.DeserializeObject<VechicleResponse>(json);

            return data;
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

            AddRequestContent(request, data);

            var response = request.GetResponse();
            var json = ResponseToString(response);
            response.Close();

            return json;
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