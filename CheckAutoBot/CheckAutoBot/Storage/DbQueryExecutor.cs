using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Storage
{
    public class DbQueryExecutor
    {
        private IRepositoryFactory _repositoryFactory;
        private ILogger _logger;

        public DbQueryExecutor(IRepositoryFactory repositoryFactory, ILogger logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
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
                _logger.Error(ex, $"Ошибка в БД при получении запроса с идентификатором {requestId}");
                return null;
            }
        }

        /// <summary>
        /// Обновление вин кода авто
        /// </summary>
        /// <param name="requestObjectId">Идентификатор объекта запроса</param>
        /// <param name="vin">Вин код</param>
        /// <returns></returns>
        public async Task UpdateVinCode(int requestObjectId, string vin)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.UpdateVinCode(requestObjectId, vin).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка в БД при обновлении VIN кода в объекте запроса.  RequestObjectId: {requestObjectId}, vin: {vin}");
            }
        }

        /// <summary>
        /// Запрос последнего объекта запроса пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetLastUserRequestObject(userId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка в БД при получении последнего объекта запроса пользователя({userId})");
                return null;
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
                _logger.Error(ex, $"Ошибка в БД при добавлении объекта запроса. UserId: {requestObject.UserId}");
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
                _logger.Error(ex, $"Ошибка в БД при сохранении запроса пользователя. RequestObjectId: {userRequest.RequestObjectId}");
                return null;
            }
        }

        /// <summary>
        /// Отметить запрос выполненным
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task MarkRequestCompleted(int requestId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.MarkRequestCompleted(requestId).ConfigureAwait(false);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Ошибка в БД при установке IsCompleted = true для запроса с идентификатором {requestId}");
            }
        }

        /// <summary>
        /// Получить типы выполненных запросов
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RequestType>> GetExecutedRequestTypes(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetExecutedRequestTypes(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка в БД при получении типов выполненных запросов для RequestObject с id: {requestObjectId}");
                return null;
            }
        }
    }
}
