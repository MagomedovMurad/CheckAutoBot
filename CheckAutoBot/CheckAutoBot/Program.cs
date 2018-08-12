using Akka.Actor;
using CheckAutoBot.Actors;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using System;
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
            var recaptcha = new Rucaptcha();

            var captchaRequest = recaptcha.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl);
            string captcha = recaptcha.GetCapthaResult(captchaRequest.Id);

            var data = rsa.GetPolicy(captcha.Substring(3, captcha.Length - 3), DateTime.Now, lp: "Р928УТ26");


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
    }
}
