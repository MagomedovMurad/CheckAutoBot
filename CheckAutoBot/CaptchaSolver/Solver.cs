using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

            Task.Run(async () =>
            {
                var html = _page.Replace("#captchaid", id)
                            .Replace("#action", action)
                            .Replace("#datasitekey", datasitekey);
                var data = Encoding.UTF8.GetBytes(html);
                _cacheController.Add(id, data, pingback);
                Process.Start("cmd", $"/C start http://{pageUrl}?id={id}").WaitForExit();
                SetCursorPos(50, 390);
                await Task.Delay(1000);
                DoMouseClick();
            });
           
            return id;
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_MOVE = 0x0001;

        public void DoMouseClick()
        {
            //perform click            
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
