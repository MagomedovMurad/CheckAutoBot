using CaptchaSolver.Infrastructure.Enums;
using CaptchaSolver.Infrastructure.Models;
using CaptchaSolver.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Server.Utils
{
    public interface ICaptchaTasksCache
    {
        string Add(string pingback, object captchaData, CaptchaType captchaType);

        string GetPingback(string id);

        string GetResult(string id);

        bool SetResult(string id, string result);

        CaptchaTask GetNextTask();
    }
    public class CaptchaTasksCache: ICaptchaTasksCache
    {
        private ConcurrentBag<CaptchaTasksCacheItem> _tasks;

        public CaptchaTasksCache()
        {
            _tasks = new ConcurrentBag<CaptchaTasksCacheItem>();
        }

        public string Add(string pingback, object captchaData, CaptchaType captchaType)
        {
            var id = Guid.NewGuid().ToString(); 
            var request = new CaptchaTasksCacheItem()
            {
                Id = id,
                InputData = captchaData,
                Pingback = pingback,
                TaskState = Enums.CaptchaTaskState.New,
                DateTime = DateTime.Now,
                CaptchaType = captchaType
            };

            _tasks.Add(request);
            return id;
        }

        public string GetResult(string id)
        {
            var task = _tasks.SingleOrDefault(x => x.Id == id);
            return task.Result;
        }

        public bool SetResult(string id, string result)
        {
            var item = _tasks.SingleOrDefault(x => x.Id == id);

            if (item == null)
                return false;

            item.Result = result;
            item.TaskState = Enums.CaptchaTaskState.Ready;
            return true;
        }

        public string GetPingback(string id)
        {
            var request = _tasks.Single(x => x.Id == id);
            return request.Pingback;
        }
         
        public CaptchaTask GetNextTask()
        {
            var item = _tasks.OrderBy(x => x.DateTime).FirstOrDefault();
            item.TaskState = Enums.CaptchaTaskState.InProcess;
            return new CaptchaTask(item.Id, item.CaptchaType, item.InputData);
        }

       
    }
}
