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
            var accessToken = "455d8d726b53a60bcee02cc228f464a67e5df040a65ea61f1561e6e87d6fc4410fae5a38c062f3d734444";

            HttpWebRequest request = WebRequest.CreateHttp("http://check.gibdd.ru/proxy/check/auto/images/cache/0108.png");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var photoBinaryData = response.ReadDataAsByteArray();
            response.Close();

            var serverData = Photos.GetMessagesUploadServer("192287910", accessToken);

            var photoData = Photos.UploadPhotoToServer(serverData.UploadUrl, photoBinaryData);

            var photo = Photos.SaveMessagesPhoto(photoData, accessToken);

            var messageParams = new SendMessageParams()
            {
                PeerId = 102769356,
                Message = "Test message",
                Attachments = $"photo{photo.OwnerId}_{photo.Id}",
                AccessToken = accessToken
            };

            CheckAutoBot.Vk.Api.Messages.Send(messageParams);

            Console.ReadKey();




            //RequestHelper.AddRequestContent(request, data);

            //WebResponse response = request.GetResponse();
            //var responseData = RequestHelper.ResponseToString(response);
            //response.Close();


            //ActorSystem actorSystem = ActorSystem.Create("TestSystem");
            //var server = actorSystem.ActorOf(Props.Create(typeof(ServerActor)), "ServerActor");
            //server.Tell(new StartServerMessage());

            //var action = new ButtonAction()
            //{
            //    Lable = "Test key1",  
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
            //    PeerId = 102769356,
            //    Message = "Test message",
            //    Keyboard = keyboard,
            //    AccessToken = "a53a17f1361887ba76efdaa4b8a156f2d604e0f96a92dc0c821ebe3b763e80d31a41e01ee27bbd77c7302"
            //};

            //VkApi.Messages.Send(test);

            //var rucaptcha = new Rucaptcha();
            //var guvm = new Guvm();
            //var t = guvm.GetCaptcha();


            //var fnp = new ReestrZalogov();

            //var captchaResult = fnp.GetCaptcha();
            //var captchaRequest = rucaptcha.SendImageCaptcha(captchaResult.ImageBase64);

            //var captchaAnswer = Test(captchaRequest.Id, rucaptcha);
            //fnp.GetPledges("JMBLYV97W7J004216", captchaAnswer.Answer, captchaResult.JsessionId);

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
