using Akka.Actor;
using CheckAutoBot.Actors;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckAutoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            var t = random.NextDouble() * 10000;

            //ActorSystem actorSystem = ActorSystem.Create("ActorsSystem");
            //IActorRef serverActor = actorSystem.ActorOf(Props.Create<ServerActor>(), "server");
            //serverActor.Tell(new StartServerMessage());

            //Console.ReadLine();

            var gibdd = new Gibdd();
            var rucaptcha = new Rucaptcha();

            //var captchaRequest = rucaptcha.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl);
            //string captcha = Test(captchaRequest.Id, rucaptcha);


            //var captchaResult = gibdd.GetCaptcha();
            //var id = recaptcha.SendImageCaptcha(captchaResult.ImageBase64).Result;


            //var captchaWord = recaptcha.GetCapthaResult(id.Substring(3, id.Length - 3));

            //RUMKEK938GV067592 mazda
            //XTA21150064291647 2115
            //XWB3K32EDCA235394 nexia
            //JMBLYV97W7J004216 залог


            //var data = gibdd.GetRestriction("XWB3K32EDCA235394", captchaWord.Substring(3, captchaWord.Length - 3), captchaResult.JsessionId);

           // Console.WriteLine(data);
            Console.ReadKey();
        }

        private static string Test(long id, Rucaptcha rucaptcha)
        {
            string captcha = rucaptcha.GetCapthaResult(id);
            if (captcha == "CAPCHA_NOT_READY")
                return Test(id, rucaptcha);
            else
                return captcha;
        }
    }
}
