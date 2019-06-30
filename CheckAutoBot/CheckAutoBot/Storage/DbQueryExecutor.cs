using CheckAutoBot.Utils;
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
        private ICustomLogger _logger;

        public DbQueryExecutor(IRepositoryFactory repositoryFactory, ICustomLogger logger)
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
                var error = $"Ошибка в БД при получении запроса с идентификатором {requestId}: {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
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
                var error = $"Ошибка в БД при обновлении VIN кода в объекте запроса.  RequestObjectId: {requestObjectId}, vin: {vin}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
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
                var error = $"Ошибка в БД при получении последнего объекта запроса пользователя({userId}). {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return null;
            }
        }

        public async Task<RequestObject> GetUserRequestObject(int id)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetUserRequestObject(id).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при получении объекта запроса пользователя({id}). {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
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
                var error = $"Ошибка в БД при добавлении объекта запроса. UserId: {requestObject.UserId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
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
                var error = $"Ошибка в БД при сохранении запроса пользователя. RequestObjectId: {userRequest.RequestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return null;
            }
        }


        public async Task ChangeRequestStatus(int requestId, bool? state)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.ChangeRequestStatus(requestId, state).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при изменении статуса запроса с идентификатором {requestId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
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
                var error = $"Ошибка в БД при получении типов выполненных запросов для RequestObject с id: {requestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return null;
            }
        }

        /// <summary>
        /// Проверить наличие выполняемых запросов
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
        public async Task<bool> ExistRequestsInProcess(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.ExistRequestsInProcess(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при получении типов выполненных запросов для RequestObject с id: {requestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return true;
            }
        }

        public async Task<bool> MarkAsPaid(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.MarkAsPaid(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при сохранении данных об оплате. RequestObject id: {requestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return true;
            }
        }

        /// <summary>
        /// Проверить наличие неоплаченных запросов
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
        public async Task<bool> Exist(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.ExistRequestsInProcess(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при получении типов выполненных запросов для RequestObject с id: {requestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return true;
            }
        }

        public async Task AddRequestObjectCacheItem(RequestObjectCache item)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.AddRequestObjectCacheItem(item);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при сохранении элемента кэша объекта запросов. RequestObjectId: {item.RequestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
            }
        }

        public async Task<RequestObjectCache> GetRequestObjectCacheItem(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetRequestObjectCacheItem(requestObjectId);
                }
            }
            catch (Exception ex)
            {
                var error = $"Ошибка в БД при получении элемента кэша объекта запросов. RequestObjectId: {requestObjectId}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
                return null;
            }
        }
    }
}
