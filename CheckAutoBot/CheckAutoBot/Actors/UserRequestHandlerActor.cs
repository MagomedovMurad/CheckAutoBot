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
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage request)
        {
            var requestObject = await GetLastUserRequestObject(request.UserId);

            if (requestObject == null)
                return false;

            return false;
        }

        private async Task<bool> UserInputDataMessageHandler(UserInputDataMessage message)
        {
            message.

            return false;
        }

        private void PreGetHistory()
        {
            
        }

        #region DBQueries

        private async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            using (var rep = _repositoryFactory.CreateBotRepository())
            {
                return await rep.GetLastUserRequestObject(userId).ConfigureAwait(false);
            }
        }

        private async Task<bool> AddRequestObject(RequestObject requestObject)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.AddRequestObject(requestObject).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion DBQueries

        private void Test()
        {

            var t = new Dictionary<InputDataType, Type>()
            {

            };
        }
    }
}
