using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using CaptchaSolver.Infrastructure.Models;

namespace CaptchaSolver.Client
{
    public class Server
    {
        private HttpListener _httpListener;
        private IRecaptchaV3Solver _recaptchaV3Solver;

        private const string LocalUserHostAddress = "127.0.0.1:80";

        public Server(IRecaptchaV3Solver recaptchaV3Solver)
        {
            _recaptchaV3Solver = recaptchaV3Solver;
        }

        public async Task Start()
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://{LocalUserHostAddress}/");
                _httpListener.Start();
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

                    if (request.UserHostAddress == LocalUserHostAddress)
                    {
                        if (request.HttpMethod == "POST" && request.RawUrl.StartsWith("/captchasolver/page_ready"))
                        {
                            statusCode = PageReadyHandler();
                        }
                        else if (request.HttpMethod == "POST" && request.RawUrl.StartsWith("/captchasolver/answer"))
                        {
                            statusCode = AnswerHandler(requestData);
                        }
                        else if (request.HttpMethod == "GET" && request.QueryString["id"] != null)
                        {
                            var id = request.QueryString["id"];
                            response = _recaptchaV3Solver.GetPage(id);
                            if (response == null)
                                statusCode = HttpStatusCode.Conflict;

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

        private HttpStatusCode AnswerHandler(string data)
        {
            var result = JsonConvert.DeserializeObject<CaptchaTaskResult>(data);
            _recaptchaV3Solver.SetAnswer(result);
            return HttpStatusCode.OK;
        }

        private HttpStatusCode PageReadyHandler()
        {
            _recaptchaV3Solver.StartUserWorkImitation();
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
