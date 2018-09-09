using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        public UserRequestHandlerActor()
        {
            Receive<UserRequestMessage>(x => UserRequestHandler(x));
        }

        private void UserRequestHandler(UserRequestMessage request)
        {
        }

        private void PreGetVin()
        { 
             
        }

        private void GetVin()
        {

        }

        private void PreGetHistory()
        {

        }

        private void GetHistory()
        {

        }

        private void PreGetDtp()
        {

        }

        private void GetDtp()
        {

        }
    }
}
