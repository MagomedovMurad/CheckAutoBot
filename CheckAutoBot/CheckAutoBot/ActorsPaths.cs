using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public static class ActorsPaths
    {
        public static readonly AkkaActorMetaData ServerActor = new AkkaActorMetaData("Server");
        public static readonly AkkaActorMetaData GroupEventsHandlerActor = new AkkaActorMetaData("GroupEventsHandler");
        public static readonly AkkaActorMetaData PrivateMessageHandlerActor = new AkkaActorMetaData("PrivateMessageHandler");
        public static readonly AkkaActorMetaData PrivateMessageSenderActor = new AkkaActorMetaData("PrivateMessageSender");
        public static readonly AkkaActorMetaData UserRequestHandlerActor = new AkkaActorMetaData("UserRequestHandler");
        public static readonly AkkaActorMetaData UserRequestObjectHandlerActor = new AkkaActorMetaData("UserRequestObjectHandler");
        public static readonly AkkaActorMetaData LicensePlateHandlerActor = new AkkaActorMetaData("LicensePlateHandlerActor");
        public static readonly AkkaActorMetaData InputDataHandlerActor = new AkkaActorMetaData("InputDataHandlerActor");
    }

    public interface ICanSelectActor
    {
        ICanTell ActorSelection(IUntypedActorContext context, string path);
    }

    public class ActorSelector : ICanSelectActor
    {
        public ICanTell ActorSelection(IUntypedActorContext context, string path)
        {
            return context.ActorSelection(path);
        }
    }
}
