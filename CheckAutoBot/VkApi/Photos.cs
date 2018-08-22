using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VkApi.Utils;

namespace VkApi
{
    public class Photos
    {
        public static string GetMessagesUploadServer(string peerId, string accessToken)
        {
            string url = $"https://api.vk.com/method/photos.getMessagesUploadServer";
            string stringData = $"peer_id={peerId}&access_token={accessToken}&v=5.80";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";

            RequestHelper.AddRequestContent(request, data);

            WebResponse response = request.GetResponse();
            var responseData = RequestHelper.ResponseToString(response);
            response.Close();

            return responseData;
        }
    }
}
