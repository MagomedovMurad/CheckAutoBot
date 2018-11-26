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
        public const string LpPingbackUrl = "http://95.31.241.19/bot/captcha/lp";
        public const string RequestPingbackUrl = "http://95.31.241.19/bot/captcha/request";

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl, string pingback)
        {
            var url = $"http://rucaptcha.com/in.php?key={apiKey}&method=userrecaptcha&googlekey={dataSiteKey}&pageurl={pageUrl}&pingback={pingback}&json=1";
            var json = ExecuteRequest(url, "POST");
            var data = JsonConvert.DeserializeObject<CaptchaRequest>(json);

            return data;
        }

        public CaptchaRequest SendImageCaptcha(string base64, string pingback)
        {
            string url = "http://rucaptcha.com/in.php";
            string stringData = $"?key={apiKey}&body={base64.UrlEncode()}&method=base64&pingback={pingback}&json=1";

            url += stringData;

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.KeepAlive = false;
            request.Method = "POST";

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
            request.KeepAlive = false;
            var response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();
            return json;
        }

    }
}
