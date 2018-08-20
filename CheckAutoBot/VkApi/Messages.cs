using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VkApi.MessagesModels;

namespace VkApi
{
    public class Messages
    {
        public static void Send(SendMessageParams args)
        {
            string url = $"https://api.vk.com/method/messages.send";
            string stringData = $"message={args.Message}&peer_id={args.PeerId}&keyboard={args.Keyboard}&access_token={args.AccessToken}&v=5.80";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";

            AddRequestContent(request, data);

            WebResponse response = request.GetResponse();
            var json = ResponseToString(response);
            response.Close();
        }

        private static void AddRequestContent(HttpWebRequest request, byte[] data)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }

        private static string ResponseToString(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }


    }
}
