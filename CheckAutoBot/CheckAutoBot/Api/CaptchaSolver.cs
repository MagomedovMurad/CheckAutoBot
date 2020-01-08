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
        public const string url = "http://95.31.241.19/captchasolver/start";

        public CaptchaRequest SendRecaptcha(string host, string action, string dataSitekey, string pingback)
        {
            var url = $"http://95.31.241.19/captchasolver/start?" +
                                    $"host={host}" +
                                    $"&action={action}" +
                                    $"&datasitekey={dataSitekey}" +
                                    $"&pingback={pingback}";

            var json = ExecuteRequest(url, "GET");
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
