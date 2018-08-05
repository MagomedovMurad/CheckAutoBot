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
            ActorSystem actorSystem = ActorSystem.Create("ActorsSystem");
            IActorRef serverActor = actorSystem.ActorOf(Props.Create<ServerActor>(), "server");
            serverActor.Tell(new StartServerMessage());

            Console.ReadLine();

            //var gibdd = new Gibdd();
            //var recaptcha = new Rucaptcha();

            //var captchaResult = gibdd.GetCaptha();
            //var id = recaptcha.SendImageCaptcha(captchaResult.ImageBase64).Result;


            //var captchaWord = recaptcha.GetCapthaResult(id.Substring(3,id.Length - 3 ));

            //var data = gibdd.GetHistory("X9FMXXEEBMCB65023", captchaWord.Substring(3, captchaWord.Length - 3), captchaResult.JsessionId);

            //Console.WriteLine(data);
            //Console.ReadKey();
        }
    }
}
