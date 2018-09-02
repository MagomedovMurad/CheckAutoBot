using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public static class ActorsPaths
    {
        public static readonly AkkaActorMetaData ServerActor = new AkkaActorMetaData("server");
        public static readonly AkkaActorMetaData PrivateMessageHandlerActor = new AkkaActorMetaData("privateMessageHandler");
    }

    public interface ICanSelectActor
    {
        ICanTell ActorSelection(IUntypedActorContext context, string path);
    }

    internal class ActorSelector : ICanSelectActor
    {
        public ICanTell ActorSelection(IUntypedActorContext context, string path)
        {
            return context.ActorSelection(path);
        }
    }
}
