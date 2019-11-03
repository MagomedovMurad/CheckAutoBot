using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using NLog;
using System.Linq;
using EasyNetQ;
using CheckAutoBot.Infrastructure.Messages;

namespace CheckAutoBot.Controllers
{
    public interface IRequestedCaptchasCacheController
    {
        event EventHandler<CaptchaRequestDataEnvelop> CaptchasSolved;
        void Add(int id, IEnumerable<CaptchaRequestData> captchas);
        //void SendReport(string answer, bool isGood);
        //void Remove(int id); 
    }

    public class RequestedCaptchasCacheController: IRequestedCaptchasCacheController
    {
        private List<CaptchaRequestDataEnvelop> _requestedCaptchas;
        private RucaptchaManager _rucaptchaManager;
        private readonly ICustomLogger _logger;
        private readonly IBus _bus;

        public event EventHandler<CaptchaRequestDataEnvelop> CaptchasSolved;

        private Timer _timer;

        public RequestedCaptchasCacheController(ICustomLogger logger, IBus bus, RucaptchaManager rucaptchaManager)
        {
            _requestedCaptchas = new List<CaptchaRequestDataEnvelop>();
            _rucaptchaManager = rucaptchaManager;
            _logger = logger;
            _bus = bus;

            _bus.Subscribe<CaptchaSolvedEventMessage>("2D815518-0628-4AE7-83DF-4C80E40528BF", CaptchaSolvedEventHandler);

            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void CaptchaSolvedEventHandler(CaptchaSolvedEventMessage message)
        {
            Report(message.CaptchaId, message.Answer);
        }

        public void Add(int id, IEnumerable<CaptchaRequestData> captchas)
        {
            var envelop = new CaptchaRequestDataEnvelop()
            {
                Id = id,
                CaptchaRequestDataList = captchas,
                DateTime = DateTime.Now
            };
            _requestedCaptchas.Add(envelop);
        }

        private void Report(string captchaId, string answer)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Получена каптча с идентификатором: {captchaId}");

            var envelop = _requestedCaptchas.SingleOrDefault(x => x.CaptchaRequestDataList.Select(y => y.CaptchaId).Contains(captchaId));
            if (envelop is null)
            {
                var error = $"В кэше не найдена запись с идентификатором каптчи {captchaId}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return;
            }

            var captchaRequestData = envelop.CaptchaRequestDataList.Single(x => x.CaptchaId.Equals(captchaId));
            captchaRequestData.Value = answer;

            bool notCompleted = envelop.CaptchaRequestDataList.Any(x => string.IsNullOrWhiteSpace(x.Value));

            if (notCompleted)
            {
                var debugMsg = $"Ожидание следующей каптчи для запроса с ID: {envelop.Id}";
                _logger.WriteToLog(LogLevel.Debug, debugMsg, false);
                return;
            }

            CaptchasSolved?.Invoke(this, envelop);
            _requestedCaptchas.Remove(envelop);
        }

        //private void Remove(int id)
        //{
        //    var data = _requestedCaptchas.Single(x => x.Id == id);
        //    _requestedCaptchas.Remove(data);
        //}

        //public void SendReport(string answer, bool isGood)
        //{
        //    var data = _requestedCaptchas.SelectMany(x => x.CaptchaRequestDataList).Single(x => x.Value.Equals(answer));
        //    _logger.WriteToLog(LogLevel.Debug, $"Отправка отчета по решенной каптче: решена {(isGood ? "ВЕРНО" : "НЕВЕРНО")}");
        //    _rucaptchaManager.SendReport(data.CaptchaId, isGood);
        //}


        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var captchasWithoutAnswer = _requestedCaptchas.Where(x => (DateTime.Now - x.DateTime).TotalSeconds > 180)
                                                          .SelectMany(y => y.CaptchaRequestDataList).ToList();

            foreach (var captcha in captchasWithoutAnswer)
            {
                var captchaWord = _rucaptchaManager.GetCaptchaResult(captcha.CaptchaId);

                if (captchaWord == "CAPCHA_NOT_READY")
                    continue;


                Report(captcha.CaptchaId, captchaWord);
            }
        }
    }
}
