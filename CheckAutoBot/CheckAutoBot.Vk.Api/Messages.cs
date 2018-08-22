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
            string stringData = $"message={args.Message}&peer_id={args.PeerId}&attachment={args.Attachments}&access_token={args.AccessToken}&v=5.80";
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
