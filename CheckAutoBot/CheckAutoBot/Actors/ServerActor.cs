﻿using Akka.Actor;
using CheckAutoBot.CallbackObjects;
using CheckAutoBot.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class ServerActor: ReceiveActor
    {
        HttpListener _httpListener;

        public ServerActor()
        {
            Receive<StartServerMessage>(message => Start());
            Receive<StopServerMessage>(message => Stop());
        }

        private async void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://127.0.0.1:80/bot/captha/");
            _httpListener.Prefixes.Add("http://127.0.0.1:80/bot/vk/");
            _httpListener.Prefixes.Add("http://127.0.0.1:80/test/");
            _httpListener.Start();

            while (true)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;


                if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captha/")
                {
                    var response = GetStreamData(request.InputStream, request.ContentEncoding);
                    var responseMessage = JsonConvert.DeserializeObject<CaptchaResponseMssage>(response);

                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/vk/")
                {
                    var response = GetStreamData(request.InputStream, request.ContentEncoding);
                    var message = JsonConvert.DeserializeObject<GroupEventMessage>(response);

                    context.Response.StatusCode = 200;
                    context.Response.Close();

                }
                else if (request.HttpMethod == "GET" && request.RawUrl == "/test")
                {
                    byte[] buffer = Encoding.UTF8.GetBytes("Server working");
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