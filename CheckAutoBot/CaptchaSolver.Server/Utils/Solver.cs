using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptchaSolver.Server.Utils
{
    public interface ISolver
    {
        string StartCaptchaSolved(string pageUrl, string action, string datasitekey, string pingback);
    }
    public class Solver: ISolver
    {
        private string _page;
        private ICaptchaTasksCache _cacheController;
        public Solver(ICaptchaTasksCache cacheController)
        {
            _page = File.ReadAllText(@"SolverPage.html");
            _cacheController = cacheController;
        }

        public string StartCaptchaSolved(string pageUrl, string action, string datasitekey, string pingback)
        {
            var id = Guid.NewGuid().ToString();

            Task.Run(async () =>
            {
                var html = _page.Replace("#captchaid", id)
                            .Replace("#action", action)
                            .Replace("#datasitekey", datasitekey);
                var data = Encoding.UTF8.GetBytes(html);
               // _cacheController.Add(id, data, pingback);
                Process.Start("cmd", $"/C start http://{pageUrl}?id={id}").WaitForExit();
                User32.SetCursorPosition(50, 390);
                await Task.Delay(1000);
                User32.DoMouseClick();
            });

            return id;
        }
    }
}
