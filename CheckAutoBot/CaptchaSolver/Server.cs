using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.IO;
using Newtonsoft.Json;
using CaptchaSolver.Models;
using CheckAutoBot.Infrastructure.Extensions;

namespace CaptchaSolver
{
    public class Server
    {
        private HttpListener _httpListener;
        private readonly ICustomLogger _logger;
        private ICacheController _cacheController;
        private ISolver _solver;

        private const string LocalUserHostAddress = "127.0.0.1:80";
        private const string LocalNetworkAddress = "192.168.0.103:26565";

        public Server(ICacheController cacheController, ISolver solver)
        {
            _cacheController = cacheController;
            _solver = solver;
        }

        public async Task Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://{LocalUserHostAddress}/");
            _httpListener.Prefixes.Add($"http://{LocalNetworkAddress}/captchasolver/");
            _httpListener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    HttpListenerRequest request = context.Request;

                    var requestData = GetStreamData(request.InputStream, request.ContentEncoding);
                    HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
                    byte[] response = null;

                    if (request.UserHostAddress == LocalUserHostAddress)
                    {
                        if (request.HttpMethod == "GET" && request.RawUrl.StartsWith("/captchasolver/page"))
                        {
                            var id = request.QueryString["id"];
                            response = await CaptchaSolverGetPageHandler(id);
                            statusCode = HttpStatusCode.OK;
                        }
                        else if (request.HttpMethod == "POST" && request.RawUrl.StartsWith("/captchasolver/answer"))
                        {
                            statusCode = await CaptchaSolverAnswerHandler(requestData);
                        }
                        else if (request.HttpMethod == "GET")
                        {
                            var id = request.QueryString["id"];
                            response = await CaptchaSolverGetPageHandler(id);
                            statusCode = HttpStatusCode.OK;
                        }
                    }
                    else if (request.UserHostAddress == LocalNetworkAddress)
                    {
                        if (request.HttpMethod == "GET" && request.RawUrl.StartsWith("/captchasolver/start"))
                        {
                            var host = request.QueryString["pageurl"];
                            var action = request.QueryString["action"];
                            var datasitekey = request.QueryString["datasitekey"];
                            var pingback = request.QueryString["pingback"];

                            var captchaId = _solver.StartCaptchaSolved(host, action, datasitekey, pingback);
                            response = Encoding.UTF8.GetBytes(captchaId);
                            statusCode = HttpStatusCode.OK;
                        }
                    }

                    context.Response.StatusCode = (int)statusCode;

                    if (response is null)
                        context.Response.Close();
                    else
                        context.Response.Close(response, false);
                }
                catch (Exception ex)
                {
                    //_logger.WriteToLog(LogLevel.Error, ex.ToString(), true);
                }
            }
        }

        private async Task<byte[]> CaptchaSolverGetPageHandler(string id)
        {
            return _cacheController.GetPage(id);
            //var data = File.ReadAllBytes(@"C:\Users\Мурад\Documents\Test page.html");
            //return data;
        }

        private async Task<HttpStatusCode> CaptchaSolverAnswerHandler(string data)
        {
            var answer = JsonConvert.DeserializeObject<CaptchaAnswer>(data);
            var pingback = _cacheController.GetPingback(answer.Id);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pingback);
            request.Method = "POST";
            request.KeepAlive = false;
            request.AddContent(Encoding.Default.GetBytes($"id={answer.Id}&code={answer.Token}"));
            var response = request.GetResponse();
            var json = response.ReadDataAsString();
            response.Close();

            return HttpStatusCode.OK;
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
