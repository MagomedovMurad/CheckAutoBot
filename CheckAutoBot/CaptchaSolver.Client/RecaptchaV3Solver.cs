using CaptchaSolver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CaptchaSolver.Client
{
    public interface IRecaptchaV3Solver
    {
        event EventHandler<CaptchaTaskResult> CaptchaSolved;  

        void StartCaptchaSolved(string id, string pageUrl, string action, string datasitekey);

        void StartUserWorkImitation();

        void SetAnswer(CaptchaTaskResult result);

        byte[] GetPage(string id);
    }

    public class RecaptchaV3Solver: IRecaptchaV3Solver
    {
        private string _initialPage;
        private byte[] _generatedPage;

        private string _currentId;
        
        public event EventHandler<CaptchaTaskResult> CaptchaSolved;

        public RecaptchaV3Solver()
        {
            _initialPage = File.ReadAllText(@"SolverPage.html");
        }

        public void StartCaptchaSolved(string id, string pageUrl, string action, string googleKey)
        {
            _currentId = id;
            _generatedPage = GeneratePage(id, action, googleKey);
            OpenPageInBrowser(id, pageUrl);
        }
        public void StartUserWorkImitation()
        {
            Task.Run(() =>
            {
                //TODO: Естественно переделать
                SetCursorPos(50, 390);
                DoMouseClick();
            });
        }

        public void SetAnswer(CaptchaTaskResult result)
        {
            CaptchaSolved.Invoke(this, result);
        }

        public byte[] GetPage(string id)
        {
            if (id == _currentId)
                return _generatedPage;

            return null;
        }

        private byte[] GeneratePage(string id, string action, string googleKey)
        {
            var html = _initialPage.Replace("#captchaid", id)
                               .Replace("#action", action)
                               .Replace("#datasitekey", googleKey);
            return Encoding.UTF8.GetBytes(html);
        }

        private void OpenPageInBrowser(string id, string pageUrl)
        {
            Process.Start("cmd", $"/C start http://{pageUrl}?id={id}").WaitForExit();
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
