using Akka.Serialization;
using CheckAutoBot.Captcha;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Infrastructure.Extensions;
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
        private const string apiKey = "3c0d64865e369b523fae883355223479";
        public const string LpPingbackUrl = "http://95.31.241.19/bot/captcha";
        public const string VinPingbackUrl = "http://95.31.241.19/bot/captcha";
        public const string RequestPingbackUrl = "http://95.31.241.19/bot/captcha";

        public CaptchaRequest SendReCaptcha2(string dataSiteKey, string pageUrl, string pingback)
        {
            var url = $"http://rucaptcha.com/in.php?" +
                                    $"key={apiKey}" +
                                    $"&method=userrecaptcha" +
                                    $"&googlekey={dataSiteKey}" +
                                    $"&pageurl={pageUrl}" +
                                    $"&pingback={pingback}" +
                                    $"&json=1";

            var json = ExecuteRequest(url, "POST");
            var data = JsonConvert.DeserializeObject<CaptchaRequest>(json);

            return data;
        }

        public CaptchaRequest SendReCaptcha3(string dataSiteKey, string pageUrl, string pingback, int version, string action)
        {
            var url = $"http://rucaptcha.com/in.php?" +
                                        $"key={apiKey}" +
                                        $"&method=userrecaptcha" +
                                        $"&googlekey={dataSiteKey}" +
                                        $"&pageurl={pageUrl}" +
                                        $"&pingback={pingback}" +
                                        $"&version=v{version}" +
                                        $"&action={action}" +
                                        $"&min_score=0.3" +
                                        $"&json=1";

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

        public CaptchaAnswer GetCapthaResult(string capthchaId)
        {
            var url = $"http://rucaptcha.com/res.php?key={apiKey}&action=get&id={capthchaId}&json=1";
            var json = ExecuteRequest(url, "GET");

            var result = JsonConvert.DeserializeObject<CaptchaAnswer>(json);
            return result;
        }

        public void SendReportGood(string id)
        {
            var url = $"https://rucaptcha.com/res.php?" +
                      $"key={apiKey}&action=reportgood&id={id}&json=1";

            ExecuteRequest(url, "POST");
        }

        public void SendReportBad(string id)
        {
            var url = $"https://rucaptcha.com/res.php?" +
                      $"key={apiKey}&action=reportbad&id={id}&json=1";
            ExecuteRequest(url, "POST");
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
