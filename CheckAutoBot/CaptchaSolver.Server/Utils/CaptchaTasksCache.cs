using CaptchaSolver.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Server.Utils
{
    public interface ICaptchaTasksCache
    {
        string Add<TCaptchaData>(string pingback, TCaptchaData captchaData);
        string GetPingback(string id);

        string GetResult(string id);
    }
    public class CaptchaTasksCache: ICaptchaTasksCache
    {
        private List<CaptchaTask<object>> _tasks;

        public CaptchaTasksCache()
        {
            _tasks = new List<CaptchaTask<object>>();
        }

        public string Add<TCaptchaData>(string pingback, TCaptchaData captchaData)
        {
            var id = Guid.NewGuid().ToString(); 
            var request = new CaptchaTask<object>()
            {
                Id = id,
                Data = captchaData,
                Pingback = pingback
            };

            _tasks.Add(request);
            return id;
        }

        public string GetResult(string id)
        {
            var request = _tasks.SingleOrDefault(x => x.Id == id);
            return request.Result;
        }

        public string GetPingback(string id)
        {
            var request = _tasks.Single(x => x.Id == id);
            return request.Pingback;
        }
    }
}
