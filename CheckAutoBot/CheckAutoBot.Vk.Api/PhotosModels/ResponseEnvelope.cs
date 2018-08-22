using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Vk.Api.PhotosModels
{
    public class ResponseEnvelope<T>
    {
        [JsonProperty("response")]
        public T Envelope {get; set;}
    }
}
