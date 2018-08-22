using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace VkApi.Utils
{
    public static class RequestHelper
    {
        public static void AddRequestContent(HttpWebRequest request, byte[] data)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }

        public static string ResponseToString(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        public static byte[] ResponseToByteArray(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
