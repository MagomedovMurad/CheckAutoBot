using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptchaSolver.Server.Models;
using CaptchaSolver.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            return _tasksCache.Add(pingback, data);
        }

        [HttpGet("result")]
        public string ResultGet(string id)
        {
            var result = _tasksCache.GetResult(id);
            return result ?? "ХЗ почему, но ответа нет. Можете попробовать ещё раз.";
        }

        #endregion

        #region  SolverClient

        [HttpGet("next_task")]
        public string NextTaskGet()
        {
            return "next_task";
        }

        [HttpPost("task_result")]
        public string TaskResultPost()
        {
            return "task_result";
        }

        #endregion
    }
}
