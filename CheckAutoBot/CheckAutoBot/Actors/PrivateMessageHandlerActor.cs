using Akka.Actor;
using CheckAutoBot.Vk.Api.MessagesModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class PrivateMessageHandlerActor: ReceiveActor
    {
        public PrivateMessageHandlerActor()
        {
            Receive<PrivateMessage>(x => MessageHandler(x));
        }

        public void MessageHandler(PrivateMessage message)
        {
            if (message.Payload == null)
            {

            }
            else
            {
 
            }
        }

        private void AddRequestObject()
        {
            var requestObject = new RequestObject()
            {

            }
        }
    }
}
