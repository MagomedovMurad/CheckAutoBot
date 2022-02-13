using CheckAutoBot.Utils;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NLog;
using System.Threading.Tasks;
using System.IO;
using CheckAutoBot.Api;
using CheckAutoBot.YandexMoneyModels;
using CheckAutoBot.Infrastructure.Messages;
using CheckAutoBot.Controllers;
using CheckAutoBot.Messages;

namespace CheckAutoBot
{
    public interface IServer
    {
        Task Start();
        void Stop();
    }

    public class Server: IServer
    {
        private HttpListener _httpListener;
        private IGroupEventsController _groupEventsController;
        private IYandexMoneyController _yandexMoneyController;
        private readonly ICustomLogger _logger;
        private readonly IBus _bus;

        public Server(ICustomLogger logger, IBus bus, IGroupEventsController groupEventsController, IYandexMoneyController yandexMoneyController)
        {
            _bus = bus;
            _logger = logger;
            _groupEventsController = groupEventsController;
            _yandexMoneyController = yandexMoneyController;
        }

        public async Task Start()
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/yandexmoney/");
                _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/captcha/");
                _httpListener.Prefixes.Add("http://192.168.0.103:26565/bot/vk/");
                _httpListener.Prefixes.Add("http://192.168.0.103:26565/test/");
                _httpListener.Prefixes.Add("http://192.168.0.103:26565/test1/");
                _httpListener.Start();
                _logger.WriteToLog(LogLevel.Debug, "Server successful started");
            }
            catch (Exception ex)
            { 
            
            }

            while (true)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    HttpListenerRequest request = context.Request;

                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
                    byte[] response = null;

                    if (request.HttpMethod == "POST" && request.RawUrl == "/bot/captcha")
                    {
                        statusCode = await RucaptchaResponseHandler(requestData);
                    }
                    else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/vk")
                    {
                        //response = Encoding.UTF8.GetBytes("53fd9628");
                        //statusCode = HttpStatusCode.OK;
                        response = Encoding.UTF8.GetBytes("ok");
                        statusCode = VkEventsHandler(requestData);
                    }
                    else if (request.HttpMethod == "POST" && request.RawUrl == "/bot/yandexmoney")
                    {
                        statusCode = await YandexMoneyEventsHandler(requestData);
                    }
                    else if (request.HttpMethod == "GET" && request.RawUrl == "/test")
                    {
                        //response = File.ReadAllBytes(@"C:\Users\Мурад\Documents\Test page.html");
                        response = Encoding.UTF8.GetBytes("Hello. I working!");
                        statusCode = HttpStatusCode.OK;
                    }
                    else
                    {
                        statusCode = await DefaultRequestHandler(requestData, request);
                    }

                    context.Response.StatusCode = (int)statusCode;

                    if (response is null)
                        context.Response.Close();
                    else
                        context.Response.Close(response, false);
                }
                catch (Exception ex)
                {
                    _logger.WriteToLog(LogLevel.Error, ex.ToString(), true);
                    await Task.Delay(1000);
                }
            }
        }

        public void Stop()
        {
            _httpListener.Stop();
            _logger.WriteToLog(LogLevel.Debug, "Server succesful stoped", true);
        }


        #region Request handlers

        private async Task<HttpStatusCode> RucaptchaResponseHandler(string data)
        {
            var message = RucaptchaParamsToCSEM(data);
            await _bus.PublishAsync(message);
            return HttpStatusCode.OK;
        }

        private HttpStatusCode VkEventsHandler(string data)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Новое событие БОТа: {data}");
            _groupEventsController.HandleGroupEvent(data);
            return HttpStatusCode.OK;
        }

        private async Task<HttpStatusCode> YandexMoneyEventsHandler(string data)
        {
            var payment = YandexMoney.ConvertToPayment(data);
            var isValid = payment.IsValid(YandexMoney.Secret);

            _yandexMoneyController.HandlePayment(payment, isValid);

            return isValid? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }

        private async Task<HttpStatusCode> DefaultRequestHandler(string data, HttpListenerRequest request)
        {
            var requestInfo = $"URL: {request.Url.OriginalString}{Environment.NewLine}" +
                                     $"Raw URL: {request.RawUrl}{Environment.NewLine}" +
                                     $"Query: {request.QueryString}{Environment.NewLine}" +
                                     $"Referred by: {request.UrlReferrer}{Environment.NewLine}" +
                                     $"HTTP Method: {request.HttpMethod}{Environment.NewLine}" +
                                     $"Host name: {request.UserHostName}{Environment.NewLine}" +
                                     $"Host address: {request.UserHostName}{Environment.NewLine}" +
                                     $"Host address: {request.UserHostAddress}{Environment.NewLine}" +
                                     $"User agent: {request.UserAgent}{Environment.NewLine}" +
                                     data;

            _logger.WriteToLog(LogLevel.Warn, $"Received from Unknow: {requestInfo}", true);

            return HttpStatusCode.Unauthorized;
        }

        #endregion

        #region Helpers

        private CaptchaSolvedEventMessage RucaptchaParamsToCSEM(string stringParams)
        {
            var requestParams = ParseRequestParams(stringParams);

            var message = new CaptchaSolvedEventMessage()
            {
                CaptchaId = requestParams["id"],
                Answer = requestParams["code"]
            };

            return message;
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

        private string GetStreamData(Stream stream, Encoding encoding)
        {
            using (var streamReader = new StreamReader(stream, encoding))
            {
                return streamReader.ReadToEnd();
            }
        }

        #endregion
    }
}
