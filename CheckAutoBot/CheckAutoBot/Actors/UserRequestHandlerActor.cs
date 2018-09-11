using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        private IRepositoryFactory _repositoryFactory;

        private Rsa _rsaManager;
        private Gibdd _gibddManager;
        private ReestrZalogov _fnpManager;

        public UserRequestHandlerActor()
        {
            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserRequestObjectMessage>(x => UserRequestObjectHandler(x));
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage request)
        {
            var requestObject = await GetLastUserRequestObject(request.UserId);

            if (requestObject == null)
                return false;


        }

        private async Task<bool> UserRequestObjectHandler(UserRequestObjectMessage requestObject)
        {
            throw new Exception();
        }

        private void PreGetHistory()
        {
            
        }

        private async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            using (var rep = _repositoryFactory.CreateBotRepository())
            {
                return await rep.GetLastUserRequestObject(userId).ConfigureAwait(false);
            }
        }

    }
}
