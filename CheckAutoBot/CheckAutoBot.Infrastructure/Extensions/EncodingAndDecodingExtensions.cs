using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CheckAutoBot.Infrastructure.Extensions
{
    public static class EncodingAndDecodingExtensions
    {
        public static string UrlEncode(this string data)
        {
            return WebUtility.UrlEncode(data);
        }

        public static string UrlDecode(this string data)
        {
            return WebUtility.UrlDecode(data);
        }
    }
}
