using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Managers
{
    public class Rsa
    {
        public Rsa()
        {
        }

        private const string policyUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm";
        private const string osagoVehicleUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/osagovehicle.htm";

        public void GetPolicyInfo(string captcha, DateTime date, string lp = "", string vin = "", string bodyNumber = "", string chassisNumber = "")
        {
            var stringData = $"vin={vin}&lp={lp}&date={date.Date.ToString("dd.mm.yyyy")}&bodyNumber={bodyNumber}&chassisNumber={chassisNumber}&captcha={captcha}";
            var data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(policyUrl);
            request.Method = "POST";
            var webRequestCollection = new WebHeaderCollection();
            webRequestCollection.Add(HttpRequestHeader.Accept, "application/json");
            webRequestCollection.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            webRequestCollection.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            webRequestCollection.Add(HttpRequestHeader.Connection, "keep-alive");
            webRequestCollection.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            webRequestCollection.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            webRequestCollection.Add("Origin", "https://dkbm-web.autoins.ru");
            webRequestCollection.Add("Referer", "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm");
            webRequestCollection.Add("X-Requested-With", "XMLHttpRequest");
            webRequestCollection.Add(HttpRequestHeader.Host, "dkbm-web.autoins.ru");
            webRequestCollection.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            request.Headers = webRequestCollection;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = request.GetResponse();
            string source;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    source = reader.ReadToEnd();
                }
            }
            response.Close();
        }

        public string GetDataSiteKey()
        {
            string url = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policy.htm";
            string stringHtml = ExecuteRequest(url, "GET");

            var html = new HtmlDocument();
            html.LoadHtml(stringHtml);

            HtmlNode bodyNode = html.DocumentNode.SelectSingleNode("//div[@class='g-recaptcha']");
            var dataSiteKey = bodyNode.Attributes["data-sitekey"].Value;

            return dataSiteKey;
        }

        private string ExecuteRequest(string url, string requestMethod)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = requestMethod;
            var response = request.GetResponse();
            string responseData;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseData = reader.ReadToEnd();
                }
            }
            response.Close();
            return responseData;
        }
    }
}
