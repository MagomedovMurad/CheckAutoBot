using CheckAutoBot.Captcha;
using CheckAutoBot.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Api
{
    public class CaptchaSolver
    {
        public const string url = "http://95.31.241.19:5000/captchasolver/start";

        public CaptchaRequest SendRecaptcha(string pageUrl, string action, string googlekey, string pingback)
        {
            var url = $"http://95.31.241.19:5000/captchasolver/start?" +
                                    $"pageurl={pageUrl}" +
                                    $"&action={action}" +
                                    $"&googlekey={googlekey}" +
                                    $"&pingback={pingback}";

            var json = ExecuteRequest(url, "POST");
            return new CaptchaRequest() { Id = json, State = true };
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
