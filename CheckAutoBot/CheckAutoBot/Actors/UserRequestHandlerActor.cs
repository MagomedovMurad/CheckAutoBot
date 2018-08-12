using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
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
                case UserRequestType.Vin:
                    break;
                case UserRequestType.History:
                    break;
                case UserRequestType.Dtp:
                    break;
                case UserRequestType.Wanted:
                    break;
                case UserRequestType.Restricted:
                    break;
                case UserRequestType.Pledge:
                    break;
                case UserRequestType.CheckUserArrears:
                    break;
                case UserRequestType.CheckUserPassport:
                    break;
            }
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
