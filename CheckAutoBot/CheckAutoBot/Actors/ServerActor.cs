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
        IUntypedActorContext _context;
        IActorRef _self;

        public ServerActor()
        {
            _actorSelector = new ActorSelector();

            Receive<StartServerMessage>(message => Start());
            Receive<StopServerMessage>(message => Stop());

            _context = Context;
            _self = Self;
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
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    RucaptchaMessagesHandler(requestData);


                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/vk")
                {
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    context.Response.StatusCode = 200;
                    byte[] buffer = Encoding.UTF8.GetBytes("ok");
                    context.Response.Close(buffer, false);

                    VKMessagesHandler(requestData);
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
            _actorSelector.ActorSelection(_context, ActorsPaths.GroupEventsHandlerActor.Path).Tell(json, _self);
        }

        private void RucaptchaMessagesHandler(string stringParams)
        {
            var requestParams = ParseRequestParams(stringParams);

            var message = new CaptchaResponseMessage()
            {
                CaptchaId = long.Parse(requestParams["id"]),
                Value = requestParams["code"]
            };
            _actorSelector.ActorSelection(_context, ActorsPaths.UserRequestHandlerActor.Path).Tell(message, _self);
        }

        private string GetStreamData(Stream stream, Encoding encoding)
        {
            using (var streamReader = new StreamReader(stream, encoding))
            {
                return streamReader.ReadToEnd();
            }
        }

        private Dictionary<string, string> ParseRequestParams(string stringParams)
        {
            Dictionary<string, string> requestParamsDictionary = new Dictionary<string, string>();

            var requestParams = stringParams.Split('&');
            foreach (var param in requestParams)
            {
                var paramKeyWithValue = param.Split('=');
                var key = paramKeyWithValue[0];
                var value = paramKeyWithValue[1];

                requestParamsDictionary.Add(key, value);
            }

            return requestParamsDictionary;
        }

    }
}
