using CheckAutoBot.Infrastructure;
using CheckAutoBot.Vk.Api.PhotosModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Vk.Api
{
    public class Photos
    {
        public static GetUploadServerResponse GetMessagesUploadServer(string peerId, string accessToken)
        {
            string url = $"https://api.vk.com/method/photos.getMessagesUploadServer";
            string stringData = $"peer_id={peerId}&access_token={accessToken}&v=5.80";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            var responseData = JsonConvert.DeserializeObject<ResponseEnvelope<GetUploadServerResponse>>(json);
            return responseData.Envelope;
        }

        public static UploadPhotoResponse UploadPhotoToServer(string serverUrl, byte[] photo)
        {
            HttpWebRequest request = WebRequest.CreateHttp(serverUrl);
            request.Method = "POST";
            request.AddPhotoAsMultipartData(photo);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return JsonConvert.DeserializeObject<UploadPhotoResponse>(json);
        }

        public static Photo SaveMessagesPhoto(UploadPhotoResponse photoData, string accessToken)
        {
            string url = $"https://api.vk.com/method/photos.saveMessagesPhoto";
            string stringData = $"server={photoData.Server}&hash={photoData.Hash}&photo={photoData.Photo}&access_token={accessToken}&v=5.80";
            byte[] data = Encoding.ASCII.GetBytes(stringData);

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.AddContent(data);

            WebResponse response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            var responseData = JsonConvert.DeserializeObject<ResponseEnvelope<List<Photo>>>(json);

            return responseData.Envelope[0];
        }
    }
}
