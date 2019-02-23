using CheckAutoBot.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Vk.Api
{
    public class Groups
    {
        public static bool IsMember(string groupId, int userId, string accessToken)
        {
            string url = $"https://api.vk.com/method/groups.isMember";

            string stringData = $"group_id={groupId.UrlEncode()}" +
                                $"&user_id={userId.ToString().UrlEncode()}" +
                                $"&access_token={accessToken.UrlEncode()}" +
                                $"&v=5.92";

            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";

            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();
            var responseData = JsonConvert.DeserializeObject<ResponseEnvelope<bool>>(json);
            return responseData.Envelope;
        }
    }
}
