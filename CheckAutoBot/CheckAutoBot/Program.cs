using Akka.Actor;
using CheckAutoBot.Actors;
using CheckAutoBot.Captcha;
using CheckAutoBot.Infrastructure;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Vk.Api;
using CheckAutoBot.Vk.Api.MessagesModels;
using CheckAutoBot.Vk.Api.PhotosModels;
using Newtonsoft.Json;
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
            var accessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2";

            ////  var photoBinaryData = response.ReadDataAsByteArray();

            //  var serverData = Photos.GetMessagesUploadServer("192287910", accessToken);

            //  var photoData = Photos.UploadPhotoToServer(serverData.UploadUrl, photoBinaryData);

            //  var photo = Photos.SaveMessagesPhoto(photoData, accessToken);

            //  var messageParams = new SendMessageParams()
            //  {
            //      PeerId = 102769356,
            //      Message = "Test message",
            //      Attachments = $"photo{photo.OwnerId}_{photo.Id}",
            //      AccessToken = accessToken
            //  };

            //  Vk.Api.Messages.Send(messageParams);

            //RequestHelper.AddRequestContent(request, data);

            //WebResponse response = request.GetResponse();
            //var responseData = RequestHelper.ResponseToString(response);
            //response.Close();


            ActorSystem actorSystem = ActorSystem.Create("TestSystem");
            var server = actorSystem.ActorOf(Props.Create(typeof(ServerActor)), ActorsPaths.ServerActor.Name);
            var groupEventsHandlerActor = actorSystem.ActorOf(Props.Create(typeof(GroupEventsHandlerActor)), ActorsPaths.GroupEventsHandlerActor.Name);
            var privateMessageHandlerActor = actorSystem.ActorOf(Props.Create(typeof(PrivateMessageHandlerActor)), ActorsPaths.PrivateMessageHandlerActor.Name);
            var privateMessageSenderActor = actorSystem.ActorOf(Props.Create(typeof(PrivateMessageSenderActor)), ActorsPaths.PrivateMessageSenderActor.Name);
            var userRequestHandlerActor = actorSystem.ActorOf(Props.Create(typeof(UserRequestHandlerActor)), ActorsPaths.UserRequestHandlerActor.Name);
            server.Tell(new StartServerMessage());
            Console.ReadKey();

            //var t = WebUtility.UrlEncode("Пойдем спать &#127911;");

            //var action = new ButtonAction()
            //{
            //    Lable = t,
            //    Type = ButtonActionType.Text,
            //    Payload = "{\"button\": \"2\"}"
            //};

            //var button = new Button()
            //{
            //    Action = action,
            //    Color = ButtonColor.Positive
            //};

            //var keyboard = new Keyboard()
            //{
            //    OneTime = true,
            //    Buttons = new[] { new[] { button } }
            //};

            //var test = new SendMessageParams()
            //{
            //    PeerId = 192287910,
            //    Message = "Пойдем спать &#127911;",
            //    Keyboard = keyboard,
            //    AccessToken = accessToken
            //};

            //Vk.Api.Messages.Send(test);

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
