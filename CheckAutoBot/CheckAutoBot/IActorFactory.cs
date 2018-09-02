using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public interface IActorFactory
    {
        ICanTell CreateServerActor(IUntypedActorContext context, string name);
    }

    //public class ActorFactory : IActorFactory
    //{
    //    //public ICanTell CreateServerActor(IUntypedActorContext context, string name)
    //    //{
    //    //  //  return context.ActorOf(context.System.DI().Props<ServerActor>(), name);
    //    //}
    //}
}
