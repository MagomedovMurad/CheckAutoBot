using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.PhotosModels
{
    public class GetUploadServerResponse
    {
        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("album_id")]
        public string AlbumId { get; set; }

        [JsonProperty("group_id")]
        public string GroupId { get; set; }
    }
}
