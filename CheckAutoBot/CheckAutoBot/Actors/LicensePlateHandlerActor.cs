using Akka.Actor;
using CheckAutoBot.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class LicensePlateHandlerActor: ReceiveActor
    {
        public LicensePlateHandlerActor()
        {
            Receive<UserInputDataMessage>(x => StartVinSearch(x));
        }

        private void StartVinSearch(UserInputDataMessage message)
        {

        }
    }
}
