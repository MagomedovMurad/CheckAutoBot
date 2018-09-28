using Akka.Actor;
using CheckAutoBot.Messages;
using CheckAutoBot.Vk.Api.MessagesModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class ServerActor: ReceiveActor
    { 
        private HttpListener _httpListener;
        private ICanSelectActor _actorSelector;

        public ServerActor()
        {
            _actorSelector = new ActorSelector();

            Receive<StartServerMessage>(message => Start());
            Receive<StopServerMessage>(message => Stop());
        }

        private async void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://192.168.0.103:8082/bot/captha/");
            _httpListener.Prefixes.Add("http://192.168.0.103:8082/bot/vk/");
            _httpListener.Prefixes.Add("http://192.168.0.103:8082/test/");
            _httpListener.Start();

            while (true)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captha/")
                {
                    var response = GetStreamData(request.InputStream, request.ContentEncoding);
                    RucaptchaMessagesHandler(response);

                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/vk")
                {
                    var response = GetStreamData(request.InputStream, request.ContentEncoding);
                    context.Response.StatusCode = 200;
                    context.Response.Close();

                    VKMessagesHandler(response);
                }
                else if (request.HttpMethod == "GET" && request.RawUrl == "/test")
                {
                    byte[] buffer = Encoding.UTF8.GetBytes("Hello, Marat");
                    context.Response.Close(buffer, false);
                }
                else
                {
                    context.Response.StatusCode = 501;
                    context.Response.Close();
                }
            }
        }

        private void Stop()
        {
            _httpListener.Stop();
        }

        private void VKMessagesHandler(string json)
        {
            var message = JsonConvert.DeserializeObject<PrivateMessage>(json);
            _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageHandlerActor.Path).Tell(message, Self);
        }

        private void RucaptchaMessagesHandler(string json)
        {
            var message = JsonConvert.DeserializeObject<CaptchaResponseMessage>(json);
            _actorSelector.ActorSelection(Context, ActorsPaths.UserRequestObjectHandlerActor.Path).Tell(message, Self);
        }

        private string GetStreamData(Stream stream, Encoding encoding)
        {
            using (var streamReader = new StreamReader(stream, encoding))
            {
                return streamReader.ReadToEnd();
            }
        }

    }
}
