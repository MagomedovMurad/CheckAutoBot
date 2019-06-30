using Akka.Actor;
using CheckAutoBot.Api;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Infrastructure.Messages;
using CheckAutoBot.Messages;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using CheckAutoBot.YandexMoneyModels;
using EasyNetQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckAutoBot.Actors
{
    public class ServerActor: ReceiveActor
    { 
        private HttpListener _httpListener;
        private readonly ICustomLogger _logger;
        private readonly IBus _bus;
        private readonly ICanSelectActor _actorSelector;
        private readonly IUntypedActorContext _context;
        private readonly IActorRef _self;

        public ServerActor(ICustomLogger logger, IBus bus)
        {
            _bus = bus;
            _actorSelector = new ActorSelector();
            _logger = logger;
            _context = Context;
            _self = Self;

            Receive<StartServerMessage>(message => Start());
            Receive<StopServerMessage>(message => Stop());
        }

        private async void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/yandexmoney/");
            _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/captcha/"); //request/");
            //_httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/captcha/lp/");
            //_httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/captcha/vin/");
            _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/vk/");
            _httpListener.Prefixes.Add("http://192.168.0.103:26565/test/");
            _httpListener.Start();
            _logger.WriteToLog(LogLevel.Debug, "Server succesful started");

            while (true)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captcha")
                {
                    _logger.WriteToLog(LogLevel.Debug, $"Received from Rucaptcha to {request.Url}");
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    await RucaptchaResponseHandler(requestData);

                    context.Response.StatusCode = (int)HttpStatusCode.OK; //200
                    context.Response.Close();
                }
                //else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captcha/lp")
                //{
                //    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                //    RucaptchaMessagesForLPHandler(requestData);

                //    context.Response.StatusCode = (int)HttpStatusCode.OK; //200
                //    context.Response.Close();
                //}

                //else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captcha/vin")
                //{
                //    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                //    RucaptchaMessagesForVinHandler(requestData);

                //    context.Response.StatusCode = (int)HttpStatusCode.OK; //200
                //    context.Response.Close();
                //}

                else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/vk")
                {
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    context.Response.StatusCode = (int)HttpStatusCode.OK; //200
                    byte[] buffer = Encoding.UTF8.GetBytes("ok");
                    context.Response.Close(buffer, false);

                    VKMessagesHandler(requestData);
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/yandexmoney")
                {
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    if(YandexMoneyRequestHandler(requestData))
                        context.Response.StatusCode = (int)HttpStatusCode.OK; //200                                                         
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest; //400

                    context.Response.Close();
                }
                else if (request.HttpMethod == "GET" && request.RawUrl == "/test")
                {
                    byte[] buffer = Encoding.UTF8.GetBytes("Hello. I working!");
                    context.Response.Close(buffer, false);
                }
                else
                {
                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);

                    var requestInfo = $"URL: {request.Url.OriginalString}{Environment.NewLine}" +
                                      $"Raw URL: {request.RawUrl}{Environment.NewLine}" +
                                      $"Query: {request.QueryString}{Environment.NewLine}" +
                                      $"Referred by: {request.UrlReferrer}{Environment.NewLine}" +
                                      $"HTTP Method: {request.HttpMethod}{Environment.NewLine}" +
                                      $"Host name: {request.UserHostName}{Environment.NewLine}" +
                                      $"Host address: {request.UserHostName}{Environment.NewLine}" +
                                      $"Host address: {request.UserHostAddress}{Environment.NewLine}" +
                                      $"User agent: {request.UserAgent}{Environment.NewLine}" +
                                      requestData;

                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented; //501;
                    context.Response.Close();
                    _logger.WriteToLog(LogLevel.Warn, $"Received from Unknow: {requestInfo}", true);
                }
            }
        }

        private void Stop()
        {
            _httpListener.Stop();
            _logger.WriteToLog(LogLevel.Debug, "Server succesful stoped", true);
        }

        private void VKMessagesHandler(string json)
        {
            _actorSelector.ActorSelection(_context, ActorsPaths.GroupEventsHandlerActor.Path).Tell(json, _self);
            //_logger.Debug($"VKMessagesHandler. Sending message to GroupEventsHandlerActor. Message: {json}");
        }

        private async Task RucaptchaResponseHandler(string stringParams)
        {
            var message = RucaptchaParamsToCRM(stringParams);
            await _bus.PublishAsync(message);
        }

        //private void RucaptchaMessagesHandler(string stringParams)
        //{
        //    var message = RucaptchaParamsToCRM(stringParams);
        //    _actorSelector.ActorSelection(_context, ActorsPaths.UserRequestHandlerActor.Path).Tell(message, _self);

        //    //_logger.Debug($"RucaptchaMessagesHandler. Sending message to UserRequestHandlerActor. Message: CaprchaId={message.CaptchaId} Code={message.Value}");
        //}
        //private void RucaptchaMessagesForLPHandler(string stringParams)
        //{
        //    var message = RucaptchaParamsToCRM(stringParams);
        //    _actorSelector.ActorSelection(_context, ActorsPaths.LicensePlateHandlerActor.Path).Tell(message, _self);
        //}

        //private void RucaptchaMessagesForVinHandler(string stringParams)
        //{
        //    var message = RucaptchaParamsToCRM(stringParams);
        //    _actorSelector.ActorSelection(_context, ActorsPaths.VinCodeHandlerActor.Path).Tell(message, _self);
        //}

        private bool YandexMoneyRequestHandler(string parameters)
        {
            var payment = YandexMoney.ConvertToPayment(parameters);
            var isValid = payment.IsValid(YandexMoney.Secret);

            var request = new PaymentRequest() { Payment = payment, IsValid = isValid};
            _actorSelector.ActorSelection(_context, ActorsPaths.YandexMoneyRequestHandlerActor.Path).Tell(request, _self);

            return isValid;
        }

        private CaptchaResponseMessage RucaptchaParamsToCRM(string stringParams)
        {
            var requestParams = ParseRequestParams(stringParams);

            var message = new CaptchaResponseMessage()
            {
                CaptchaId = requestParams["id"],
                Value = requestParams["code"]
            };

            return message;
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
