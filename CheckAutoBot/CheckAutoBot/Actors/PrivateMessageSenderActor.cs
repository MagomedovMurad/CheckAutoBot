using Akka.Actor;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageSenderActor: ReceiveActor
    {
        public PrivateMessageSenderActor()
        {
            Receive<PrivateMessage>(x => SendMsg(x));
        }

        private void SendMsg(PrivateMessage msg)
        {

        }
    }
}
