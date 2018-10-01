using Akka.Serialization;
using CheckAutoBot.Captcha;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot
{
    public class Rucaptcha
    {
        private const string apiKey = "a57666df25735811384576da1a50bf36";
        private const string pingbackUrl = "95.31.241.19/bot/captha/";

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl)
        {
            var url = $"http://rucaptcha.com/in.php?key={apiKey}&method=userrecaptcha&googlekey={dataSiteKey}&pageurl={pageUrl}&pingback={pingbackUrl}&json=1";
            var json = ExecuteRequest(url, "POST");
            var data = JsonConvert.DeserializeObject<CaptchaRequest>(json);

            return data;
        }

        public CaptchaRequest SendImageCaptcha(string base64)
        {
            string url = "http://rucaptcha.com/in.php";

            var encodedBase64 = WebUtility.UrlEncode(base64);

            string stringData = $"key={apiKey}&body={encodedBase64}&method=base64&pingback={pingbackUrl}&json=1";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "rucaptcha.com");
            headers.Add(HttpRequestHeader.Connection, "Keep-Alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.Headers = headers;

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return JsonConvert.DeserializeObject<CaptchaRequest>(json);
        }

        public CaptchaAnswer GetCapthaResult(long capthchaId)
        {
            var url = $"http://rucaptcha.com/res.php?key={apiKey}&action=get&id={capthchaId}&json=1";
            var json = ExecuteRequest(url, "GET");

            var result = JsonConvert.DeserializeObject<CaptchaAnswer>(json);
            return result;
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
