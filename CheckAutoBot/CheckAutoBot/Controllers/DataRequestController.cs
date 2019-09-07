using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using CheckAutoBot.Models.Captcha;
using CheckAutoBot.Models.RequestedDataCache;
using CheckAutoBot.Exceptions;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Infrastructure.Enums;

namespace CheckAutoBot.Controllers
{
    public interface IDataRequestController
    {
        Task StartDataSearch(int id, DataType dataType, object inputData, Func<DataRequestResult, Task> callBack);
    }

    public class DataRequestController: IDataRequestController
    {
        private IDataSource[] _dataSources;
        private IRequestedDataCacheController _requestsCache;
        private IRequestedCaptchasCacheController _captchasCacheController;
        private readonly ICustomLogger _logger;

        public DataRequestController(IDataSource[] dataSources,
                                     IRequestedCaptchasCacheController captchasCacheController,
                                     IRequestedDataCacheController requestsCache,
                                     ICustomLogger logger)
        {
            _dataSources = dataSources;
            _logger = logger;
            _requestsCache = requestsCache;
            _captchasCacheController = captchasCacheController;
            _captchasCacheController.CaptchasSolved += SolvedCaptchasHandler;
        }

        public async Task StartDataSearch(int id, DataType dataType, object inputData, Func<DataRequestResult, Task> callBack)
        {
            try
            {
                var dataSource = _dataSources.Where(x => x.DataType.Equals(dataType)).FirstOrDefault(x => x.Order.Equals(1));
                _requestsCache.Add(id, dataSource, inputData, callBack);

                RequestData(id, inputData, dataSource);
            }
            catch (Exception ex)
            {

            }
        }
        private void SolvedCaptchasHandler(object sender, CaptchaRequestDataEnvelop envelop)
        {
            var dataRequest = _requestsCache.Get(envelop.Id);
            GetDataFromSource(envelop.Id, dataRequest.DataSource, dataRequest.InputData, envelop.CaptchaRequestDataList);
        }

        private void RepeatRequest(int id, bool selectNextSource = false)
        {
            var dataRequest = _requestsCache.Get(id);

            if (dataRequest.RepeatCount >= dataRequest.DataSource.MaxRepeatCount || selectNextSource)
            {
                var dataSource = _dataSources.Where(x => x.DataType.Equals(dataRequest.DataSource.DataType))
                                             .FirstOrDefault(x => x.Order.Equals(dataRequest.DataSource.Order + 1));

                if (dataSource is null)
                {
                    ReturnData(id, null, null, null, false);
                }
                else
                {
                    _requestsCache.UpdateDataSource(id, dataSource);
                    RequestData(id, dataRequest.InputData, dataSource);
                }
            }
            else
            {
                _requestsCache.UpRepeatCount(id);
                RequestData(id, dataRequest.InputData, dataRequest.DataSource);
            }
        }

        private void RequestData(int id, object inputData, IDataSource dataSource)
        {
            if (dataSource is IDataSourceWithCaptcha dataSourceWithCaptcha)
            {
                RequestCaptcha(id, dataSourceWithCaptcha);
            }
            else if (dataSource is IDataSource dataSourceWithoutCaptcha)
            {
                GetDataFromSource(id, dataSourceWithoutCaptcha, inputData, null);
            }
        }

        private async Task RequestCaptcha(int id, IDataSourceWithCaptcha dataSource)
        {
            try
            {
                var captchas = dataSource.RequestCaptcha();
                _captchasCacheController.Add(id, captchas);
                return;
            }
            catch (InvalidOperationException ex)
            {
                var error = $"Ошибка при запросе каптчи. {Environment.NewLine}" +
                            $"Идентификатор запроса: {id}. {Environment.NewLine}" +
                            $"{ex.Message}. {ex.InnerException?.Message}";
                _logger.WriteToLog(LogLevel.Warn, error, false);
            }
            catch (Exception ex)
            {
                var error = $"Ошибка при запросе каптчи. {Environment.NewLine}" +
                            $"Идентификатор запроса: {id}.{Environment.NewLine}" +
                            $"Тип данных: {dataSource.DataType}." +
                            $"Ошибка: {ex.Message}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error);
            }

            RepeatRequest(id);
        }
        private async Task GetDataFromSource(int id, IDataSource dataSource, object inputData, IEnumerable<CaptchaRequestData> captchas)
        {
            try
            {
                var dataSourceResult = dataSource.GetData(inputData, captchas);
                ReturnData(id, dataSourceResult, dataSource.DataType, dataSource.Name, true);
                return;
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                {
                    var warn = $"Неверно решена каптча. {Environment.NewLine}" +
                               $"Ответ: {icEx.CaptchaWord}. " +
                               $"Тип данных: { dataSource.DataType}." +
                               $"Ошибка: {ex.Message}. {ex.InnerException?.Message}";
                    _logger.WriteToLog(LogLevel.Warn, warn, true);
                }
                //else if (ex is DataNotFoundException nfEx)
                //{
                //    var warn = $"В источнике данных типа {dataSource.DataType} не найдены данные. {Environment.NewLine}";
                //    _logger.WriteToLog(LogLevel.Warn, warn, false);
                //    RepeatRequest(id, true);
                //    return;
                //}
                else
                {
                    var error = $"Идентификатор запроса: {id}.{Environment.NewLine}" +
                                $"Тип данных: {dataSource.DataType}." +
                                $"Ошибка: {ex.Message}. {ex.InnerException?.Message}";
                    _logger.WriteToLog(LogLevel.Error, error);
                }
            }
            catch (Exception ex)
            {
                var error = $"Непредвиденная ошибка при запросе данных. {Environment.NewLine}" +
                            $"Идентификатор запроса: {id}.{Environment.NewLine}" +
                            $"Тип данных: {dataSource.DataType}.{Environment.NewLine}" +
                            $"Ошибка: {ex.Message}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error);
            }
            RepeatRequest(id);
        }
        private void ReturnData(int id, DataSourceResult dataSourceResult, DataType? dataType, string dataSourceName, bool isSuccessfull)
        {
            var dataRequest = _requestsCache.Get(id);

            var result = new DataRequestResult()
            {
                Id = id,
                DataSourceResult = dataSourceResult,
                DataType = dataType,
                IsSuccessfull = isSuccessfull,
                DataSourceName = dataSourceName
            };

            _requestsCache.Remove(id);
            dataRequest.CallBack(result);
        }
    }

   

    

    

}
