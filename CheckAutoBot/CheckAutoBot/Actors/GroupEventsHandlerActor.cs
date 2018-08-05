using Akka.Actor;
using CheckAutoBot.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class GroupEventsHandlerActor : ReceiveActor
    {
        public GroupEventsHandlerActor()
        {
            Receive<GroupEventMessage>(x => GroupEventMessageHandler(x));
        }

        private void GroupEventMessageHandler(GroupEventMessage message)
        {
        }
    }
}
