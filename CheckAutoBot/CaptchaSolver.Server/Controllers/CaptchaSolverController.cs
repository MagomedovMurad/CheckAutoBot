using System.Net;
using System.Text;
using System.Threading.Tasks;
using CaptchaSolver.Infrastructure.Enums;
using CaptchaSolver.Infrastructure.Extensions;
using CaptchaSolver.Infrastructure.Models;
using CaptchaSolver.Server.Models;
using CaptchaSolver.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CaptchaSolver.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CaptchaSolverController : ControllerBase
    {
        private readonly ILogger<CaptchaSolverController> _logger;
        private readonly ICaptchaTasksCache _tasksCache;

        public CaptchaSolverController(ILogger<CaptchaSolverController> logger, ICaptchaTasksCache tasksCache)
        {
            _logger = logger;
            _tasksCache = tasksCache;
        }

        #region User

        [HttpPost("start")]
        public string StartPost(string pageurl, string action, string googlekey, string pingback)
        {
            var data = new RecaptchaV3Data(pageurl, action, googlekey);
            return _tasksCache.Add(pingback, data, CaptchaType.RecaptchaV3);
        }

        [HttpGet("result")]
        public string ResultGet(string id)
        {
            var result = _tasksCache.GetResult(id);
            return result ?? "ХЗ почему, но ответа нет. Можете попробовать ещё раз. Не уверен, что это поможет, но мало ли...";
        }

        #endregion

        #region  SolverClient

        [HttpGet("next_task")]
        public CaptchaTask NextTaskGet()
        {
            var task = _tasksCache.GetNextTask();
            if (task == null)
                HttpContext.Response.StatusCode = 404;

            return task;
        }

        [HttpPost("task_result")]
        public void TaskResultPost([FromBody] CaptchaTaskResult taskResult)
        {
            _tasksCache.SetResult(taskResult.Id, taskResult.Result);
            HttpContext.Response.StatusCode = 200;

            Task.Run(() =>
            {
                var pingback = _tasksCache.GetPingback(taskResult.Id);
                //var requestJson = JsonConvert.SerializeObject(taskResult);
                var requestJson = $"id={taskResult.Id}&code={taskResult.Result}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pingback);
                request.Method = "POST";
                request.KeepAlive = false;
                request.AddContent(Encoding.Default.GetBytes(requestJson));
                var response = request.GetResponse();
                var responseJson = response.ReadDataAsString();
                response.Close();
            });
        }

        #endregion
    }
}
