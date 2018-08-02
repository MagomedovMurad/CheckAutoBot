using CheckAutoBot.Managers;
using System;
using System.Linq;
using System.Threading;

namespace CheckAutoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var gibdd = new Gibdd();
            var recaptcha = new Rucaptcha();

            var captchaResult = gibdd.GetCaptha();
            var id = recaptcha.SendImageCaptcha(captchaResult.ImageBase64).Result;


            var captchaWord = recaptcha.GetCapthaResult(id.Substring(3,id.Length -3 ));

            gibdd.GetHistory("X9FMXXEEBMCB65023", captchaWord.Substring(3, captchaWord.Length - 3), captchaResult.SessionId);


            Console.ReadKey();
        }
    }
}
