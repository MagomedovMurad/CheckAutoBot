using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public class DbQueryExecutor
    {
        private IRepositoryFactory _repositoryFactory;

        public DbQueryExecutor(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }


        /// <summary>
        /// Получить запрос пользователя
        /// </summary>
        /// <param name="requestId">Идентификатор пользователя</param>
        /// <returns></returns>
        public async Task<Request> GetUserRequest(int requestId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetUserRequest(requestId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// Обновление вин кода авто
        /// </summary>
        /// <param name="requestObjectId">Идентификатор объекта запроса</param>
        /// <param name="vin">Вин код</param>
        /// <returns></returns>
        public void UpdateVinCode(int requestObjectId, string vin)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    rep.UpdateVinCode(requestObjectId, vin).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Запрос последнего объекта запроса пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            using (var rep = _repositoryFactory.CreateBotRepository())
            {
                return await rep.GetLastUserRequestObject(userId).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Добавление объекта запроса  
        /// </summary>
        /// <param name="requestObject">Объект запроса</param>
        /// <returns></returns>
        public async Task<bool> AddRequestObject(RequestObject requestObject)
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

        /// <summary>
        /// Добавление запроса
        /// </summary>
        /// <param name="userRequest">Запрос пользователя</param>
        /// <returns></returns>
        public async Task<int?> SaveUserRequest(Request userRequest)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.AddUserRequest(userRequest);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
