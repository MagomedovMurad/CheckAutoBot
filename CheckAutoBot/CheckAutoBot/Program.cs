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
            //ActorSystem actorSystem = ActorSystem.Create("ActorsSystem");
            //IActorRef serverActor = actorSystem.ActorOf(Props.Create<ServerActor>(), "server");
            //serverActor.Tell(new StartServerMessage());

            //Console.ReadLine();

            var gibdd = new Gibdd();
            var rsa = new Rsa();
            var rucaptcha = new Rucaptcha();

            //var captchaRequest = rucaptcha.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl);
            //string captcha = Test(captchaRequest.Id, rucaptcha);

            var data = rsa.GetPolicy("", DateTime.Now, lp: "Р928УТ26");

            var policy = data.Policies[0];

            var data1 = rsa.GetPolicyInfo(policy.Serial, policy.Number, DateTime.Now, "");

            //var captchaResult = gibdd.GetCaptcha();
            //var id = recaptcha.SendImageCaptcha(captchaResult.ImageBase64).Result;


            //var captchaWord = recaptcha.GetCapthaResult(id.Substring(3, id.Length - 3));

            //RUMKEK938GV067592 mazda
            //XTA21150064291647 2115
            //XWB3K32EDCA235394 nexia
            //var data = gibdd.GetRestriction("XWB3K32EDCA235394", captchaWord.Substring(3, captchaWord.Length - 3), captchaResult.JsessionId);

            Console.WriteLine(data);
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
