using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CaptchaSolver.Infrastructure.Extensions
{
    public static class WebResposeExtensions
    {
        public static byte[] ReadDataAsByteArray(this WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string ReadDataAsString(this WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
