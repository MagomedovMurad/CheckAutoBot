using System;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Infrastructure.Extensions
{
    public static class HttpWebRequestExtensions
    {
        public static void AddContent(this HttpWebRequest request, byte[] data)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }

        public static void AddPhotoAsMultipartData(this HttpWebRequest request, byte[] data)
        {
            string boundary = DateTime.Now.Ticks.ToString(); // Разделитель

            request.ContentType = $"multipart/form-data; boundary={boundary}";
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] endLineData = Encoding.UTF8.GetBytes("\r\n"); // Конец строки
                byte[] boundaryData = Encoding.UTF8.GetBytes(string.Format("--{0}", boundary)); // Разделитель в байтах

                requestStream.Write(endLineData, 0, endLineData.Length);
                requestStream.Write(endLineData, 0, endLineData.Length);
                requestStream.Write(boundaryData, 0, boundaryData.Length);
                requestStream.Write(endLineData, 0, endLineData.Length);


                byte[] photoHeaderDispData = Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"photo\"; filename=\"photo-1.jpg\""); // Шапка файла
                requestStream.Write(photoHeaderDispData, 0, photoHeaderDispData.Length);
                requestStream.Write(endLineData, 0, endLineData.Length);

                byte[] photoHeaderCTypeData = Encoding.UTF8.GetBytes("Content-Type: image/png"); // Шапка файла
                requestStream.Write(photoHeaderCTypeData, 0, photoHeaderCTypeData.Length);
                requestStream.Write(endLineData, 0, endLineData.Length);
                requestStream.Write(endLineData, 0, endLineData.Length);

                requestStream.Write(data, 0, data.Length);

                requestStream.Write(endLineData, 0, endLineData.Length);
                requestStream.Write(boundaryData, 0, boundaryData.Length);
            }

        }
    }
}
