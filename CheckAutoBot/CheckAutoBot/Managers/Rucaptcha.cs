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

        public void SendReCaptcha2(string dataSiteKey, string pageUrl)
        {
            var url = $"http://rucaptcha.com/in.php?key={apiKey}&method=userrecaptcha&googlekey={dataSiteKey}&pageurl={pageUrl}&json=1";
            var json = ExecuteRequest(url, "POST");
        }

        public async Task<string> SendImageCaptcha(string data1)
        {
            string data;
            var baseAddress = new Uri("http://rucaptcha.com"); //Базовый адрес 
            var url = "/in.php"; //Нужная нам страница, на которую пойдет запрос

            using (var client = new HttpClient { BaseAddress = baseAddress })
            {

                var content = new FormUrlEncodedContent(new[] //для удобства можно использовать Dictionary<string, string>. Тогда тело будет ещё короче ["key"] = "YOUR_APIKEY", ["body"] = "BASE64_FILE"
                {
                    new KeyValuePair<string, string>("key", $"{apiKey}"),
                    new KeyValuePair<string, string>("body", $"{data1}"),
                    new KeyValuePair<string, string>("method", "base64")
                 }); //Наше тело, которое при помощи FormUrlEncodedContent закодируется в нужное нам "тело".

                var result = await client.PostAsync(url, content); //Отправляем на нужную страницу POST запрос с нашем телом, также тут используется CancellationToken для грамотной отмены async методов.
                var bytes = await result.Content.ReadAsByteArrayAsync();
                Encoding encoding = Encoding.GetEncoding("utf-8");
                data = encoding.GetString(bytes, 0, bytes.Length); //Все эти три строки добавлены тут для того, что бы получать данные в нужной нам кодировке (некоторые сервера к примеру выдают в неверной кодировке и может выдать ошибку). Вообще можно все 3 строки заменить на одну:
                                                                   //data = await result.Content.ReadAsStringAsync(); Тогда кодировка будет той, что выдает сервер.
                result.EnsureSuccessStatusCode();
            }

            return data;

        }

        public string GetCapthaResult(string capthchaId)
        {
            var url = $"http://rucaptcha.com/res.php?key={apiKey}&action=get&id={capthchaId}";
            var json = ExecuteRequest(url, "GET");
            return json;
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
