using Akka.Actor;
using CheckAutoBot.Actors;
using CheckAutoBot.Captcha;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Vk.Api;
using CheckAutoBot.Vk.Api.MessagesModels;
using CheckAutoBot.Vk.Api.PhotosModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
            LogManager.LoadConfiguration("NLog.config");
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello World");

            IRepositoryFactory repositoryFactory = new RepositoryFactory(); 
            DbQueryExecutor queryExecutor = new DbQueryExecutor(repositoryFactory, logger);

            ActorSystem actorSystem = ActorSystem.Create("TestSystem");
            var server = actorSystem.ActorOf(Props.Create(() => new ServerActor(logger)), ActorsPaths.ServerActor.Name);
            var groupEventsHandlerActor = actorSystem.ActorOf(Props.Create(() => new GroupEventsHandlerActor(logger)), ActorsPaths.GroupEventsHandlerActor.Name);
            var privateMessageHandlerActor = actorSystem.ActorOf(Props.Create(() => new PrivateMessageHandlerActor(queryExecutor, logger)), ActorsPaths.PrivateMessageHandlerActor.Name);
            var privateMessageSenderActor = actorSystem.ActorOf(Props.Create(() => new PrivateMessageSenderActor(logger)), ActorsPaths.PrivateMessageSenderActor.Name);
            var userRequestHandlerActor = actorSystem.ActorOf(Props.Create(() => new UserRequestHandlerActor(logger, queryExecutor)), ActorsPaths.UserRequestHandlerActor.Name);
            var inputDataHandlerActor = actorSystem.ActorOf(Props.Create(() => new InputDataHandlerActor(logger, queryExecutor)), ActorsPaths.InputDataHandlerActor.Name);
            var licensePlateHandlerActor = actorSystem.ActorOf(Props.Create(() => new LicensePlateHandlerActor(queryExecutor, logger)), ActorsPaths.LicensePlateHandlerActor.Name);
            var yandexMoneyRequestHandlerActor = actorSystem.ActorOf(Props.Create(() => new YandexMoneyRequestHandlerActor(queryExecutor)), ActorsPaths.YandexMoneyRequestHandlerActor.Name);
            
            server.Tell(new StartServerMessage());
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
