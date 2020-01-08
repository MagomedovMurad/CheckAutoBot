using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CaptchaSolver
{
    public interface ISolver
    {
        string StartCaptchaSolved(string pageUrl, string action, string datasitekey, string pingback);
    }

    public class Solver: ISolver
    {
        private string _page;
        private ICacheController _cacheController;
        public Solver(ICacheController cacheController)
        {
            _page = File.ReadAllText(@"SolverPage.html");
            _cacheController = cacheController;
        }

        public string StartCaptchaSolved(string pageUrl, string action, string datasitekey, string pingback)
        {
            var id = Guid.NewGuid().ToString();

            Task.Run(() =>
            {
                var html = _page.Replace("#captchaid", id)
                            .Replace("#action", action)
                            .Replace("#datasitekey", datasitekey);
                var data = Encoding.UTF8.GetBytes(html);
                _cacheController.Add(id, data, pingback);
                Process.Start("cmd", $"/C start http://{pageUrl}?id={id}");
                Cursor.Position
            });

            return id;
        }
    }
}
