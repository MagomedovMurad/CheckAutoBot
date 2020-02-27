using CaptchaSolver.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using CaptchaSolver.Infrastructure.Extensions;
using System.Text;

namespace CaptchaSolver.Client
{
    public interface IController
    {
        void Start();
    }

    public class Controller : IController, IDisposable
    {
        private readonly IRecaptchaV3Solver _recaptchaV3Solver;
        private bool _isBusy;
        private IDisposable _subscription;
        public Controller(IRecaptchaV3Solver recaptchaV3Solver)
        {
            _recaptchaV3Solver = recaptchaV3Solver;
            _recaptchaV3Solver.CaptchaSolved += RecaptchaV3Solver_CaptchaSolved;
        }

        public void Start()
        {
            _subscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                         .Subscribe(x => RequestNextTask());
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void RecaptchaV3Solver_CaptchaSolved(object sender, CaptchaTaskResult e)
        {
            var json = JsonConvert.SerializeObject(e);
            var data = Encoding.UTF8.GetBytes(json);
            SendTaskResult(data);
        }

        private void RequestNextTask()
        {
            if (_isBusy)
                return;

            _isBusy = true;
            var url = "http://95.31.241.19:5000/captchasolver/next_task";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.KeepAlive = false;
                var response = request.GetResponse();
                var json = response.ReadDataAsString();
                response.Close();

                if (json == null)
                {
                    _isBusy = false;
                    return;
                }

                var captchaTask = JsonConvert.DeserializeObject<CaptchaTask>(json);
                var strInputData = captchaTask.InputData as string;
                var data = JsonConvert.DeserializeObject<RecaptchaV3Data>(strInputData);
                _recaptchaV3Solver.StartCaptchaSolved(captchaTask.Id, data.PageUrl, data.Action, data.GoogleKey);
            }
            catch (Exception ex)
            {
                _isBusy = false;
            }
        }

        private void SendTaskResult(byte[] data)
        {
            var url = "http://95.31.241.19:5000/captchasolver/task_result";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.KeepAlive = false;
            request.MediaType = "application/json";
            request.ContentType = "application/json";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.AddContent(data);
            var response = request.GetResponse();
            response.Close();

            _isBusy = false;
        }
    }
}
