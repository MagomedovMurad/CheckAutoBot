using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using NLog;
using System.Linq;

namespace CheckAutoBot.Controllers
{
    public class RequestedCaptchasCacheController
    {
        private List<CaptchaRequestDataEnvelop> _requestedCaptchas;
        private RucaptchaManager _rucaptchaManager;
        private readonly ICustomLogger _logger;

        public event EventHandler<CaptchaRequestDataEnvelop> CaptchasSolved;

        private Timer _timer;

        public RequestedCaptchasCacheController(ICustomLogger logger)
        {
            _requestedCaptchas = new List<CaptchaRequestDataEnvelop>();
            _rucaptchaManager = new RucaptchaManager();
            _logger = logger;

            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        public void Add(int id, IEnumerable<CaptchaRequestData> captchas)
        {
            var envelop = new CaptchaRequestDataEnvelop()
            {
                Id = id,
                CaptchaRequestDataList = captchas
            };
            _requestedCaptchas.Add(envelop);
        }

        public void Report(string captchaId, string answer)
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
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var captchasWithoutAnswer = _requestedCaptchas.Where(x => (DateTime.Now - x.DateTime).TotalSeconds > 180)
                                                          .SelectMany(y => y.CaptchaRequestDataList);

            foreach (var captcha in captchasWithoutAnswer)
            {
                var captchaWord = _rucaptchaManager.GetCaptchaResult(captcha.CaptchaId);
                Report(captcha.CaptchaId, captchaWord);
            }
        }
    }
}
