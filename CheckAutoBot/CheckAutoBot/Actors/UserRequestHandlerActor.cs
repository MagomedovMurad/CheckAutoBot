using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        public UserRequestHandlerActor()
        {
            Receive<UserRequest>(x => UserRequestHandler(x));
        }

        private void UserRequestHandler(UserRequest request)
        {
            switch (request.Type)
            {
                case UserRequestType.History:
                    break;
            }
        }
    }
}
