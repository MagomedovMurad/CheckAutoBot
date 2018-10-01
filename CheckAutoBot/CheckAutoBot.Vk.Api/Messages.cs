using CheckAutoBot.Infrastructure;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Vk.Api
{
    public class Messages
    {
        public static void Send(SendMessageParams args)
        {
            string url = $"https://api.vk.com/method/messages.send";
            //var encodedMsg = WebUtility.UrlEncode(args.Message);

            string stringData = $"message={args.Message.UrlEncode()}" +
                                $"&peer_id={args.PeerId.ToString().UrlEncode()}" +
                                $"&attachment={args.Attachments.UrlEncode()}" +
                                $"&access_token={args.AccessToken.UrlEncode()}" +
                                $"&v=5.80";

            if (args.Keyboard != null)
                stringData += $"&keyboard={args.Keyboard.ToString().UrlEncode()}";

            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();
        }
    }
}
