using Akka.Actor;
using CheckAutoBot.Actors;
using CheckAutoBot.Captcha;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckAutoBot
{
    class Program
    {
        //RUMKEK938GV067592 mazda
        //XTA21150064291647 2115
        //XWB3K32EDCA235394 nexia
        //JMBLYV97W7J004216 залог
        static void Main(string[] args)
        {
            var rucaptcha = new Rucaptcha();
            var fnp = new Fnp1();


            var captchaResult = fnp.GetCaptcha();
            var captchaRequest = rucaptcha.SendImageCaptcha(captchaResult.ImageBase64);

            var captchaAnswer = Test(captchaRequest.Id, rucaptcha);
            fnp.GetPledges("JMBLYV97W7J004216", captchaAnswer.Answer, captchaResult.JsessionId);

            Console.ReadKey();
        }

        private static CaptchaAnswer Test(long id, Rucaptcha rucaptcha)
        {
            var captcha = rucaptcha.GetCapthaResult(id);
            if (!captcha.State)
                return Test(id, rucaptcha);
            else
                return captcha;
        }
    }
}
