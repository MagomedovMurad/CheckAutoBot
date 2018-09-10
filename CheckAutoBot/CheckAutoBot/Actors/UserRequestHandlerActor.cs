using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        public UserRequestHandlerActor()
        {
            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserRequestObjectMessage>(x => UserRequestObjectHandler(x));
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage request)
        {
            throw new Exception();
        }

        private async Task<bool> UserRequestObjectHandler(UserRequestObjectMessage requestObject)
        {
            throw new Exception();
        }

    }
}
