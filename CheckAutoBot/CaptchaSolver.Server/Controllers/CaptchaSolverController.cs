using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CaptchaSolver.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CaptchaSolverController : ControllerBase
    {
        private readonly ILogger<CaptchaSolverController> _logger;

        public CaptchaSolverController(ILogger<CaptchaSolverController> logger)
        {
            _logger = logger;
        }

        #region  SolverClient

        [HttpGet("nexttask")]
        public string Get()
        {
            HttpContext.Response.StatusCode = 403;
            return null;
          //  return "Hello";
        }

        [HttpPost("answer")]
        public string Post()
        {
            return null;
        }

        #endregion

        #region User



        #endregion

    }
}
