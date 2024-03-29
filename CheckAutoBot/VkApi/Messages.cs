﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VkApi.MessagesModels;
using VkApi.Utils;

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

            RequestHelper.AddRequestContent(request, data);

            WebResponse response = request.GetResponse();
            var json = RequestHelper.ResponseToString(response);
            response.Close();
        }
    }
}
