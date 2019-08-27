using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Enums;
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

namespace CheckAutoBot.Controllers
{
    public interface IDataRequestController
    {
        Task StartDataSearch(int id, DataType dataType, object inputData, Func<DataRequestResult, Task> callBack);
    }

    public class DataRequestController: IDataRequestController
    {
        private List<IDataSource> _dataSources;
        private RequestedDataCacheController _requestsCache;
        private RequestedCaptchasCacheController _captchasCacheController;

        private readonly ICustomLogger _logger;

        public DataRequestController(List<IDataSource> dataSources,
                                     RequestedCaptchasCacheController captchasCacheController,
                                     ICustomLogger logger)
        {
            _dataSources = dataSources;
            _logger = logger;
            _captchasCacheController = captchasCacheController;
            _captchasCacheController.CaptchasSolved += SolvedCaptchasHandler;
        }

        public async Task StartDataSearch(int id, DataType dataType, object inputData, Func<DataRequestResult, Task> callBack)
        {
            var dataSource = _dataSources.Where(x => x.DataType.Equals(dataType)).FirstOrDefault(x => x.Order.Equals(1));
            _requestsCache.Add(id, dataSource, inputData, callBack);

            RequestData(id, inputData, dataSource);
        }
        private void SolvedCaptchasHandler(object sender, CaptchaRequestDataEnvelop envelop)
        {
            var dataRequest = _requestsCache.Get(envelop.Id);
            GetDataFromSource(envelop.Id, dataRequest.DataSource, dataRequest.InputData, envelop.CaptchaRequestDataList);
        }
        private void RepeatRequest(int id)
        {
            var dataRequest = _requestsCache.Get(id);

            if (dataRequest.DataSource.MaxRepeatCount >= dataRequest.RepeatCount)
            {
                _requestsCache.UpRepeatCount(id);
                RequestData(id, dataRequest.InputData, dataRequest.DataSource);
            }
            else
            {
                var dataSource = _dataSources.Where(x => x.DataType.Equals(dataRequest.DataSource.DataType))
                                             .FirstOrDefault(x => x.Order.Equals(dataRequest.DataSource.Order + 1));

                if (dataSource is null)
                {
                    ReturnData(id, null, false);
                }
                else
                {
                    _requestsCache.UpdateDataSource(id, dataSource);
                    RequestData(id, dataRequest.InputData, dataSource);
                }
            }
        }
        private void RequestData(int id, object inputData, IDataSource dataSource)
        {
            if (dataSource is IDataSourceWithCaptcha dataSourceWithCaptcha)
            {
                RequestCaptcha(id, dataSourceWithCaptcha);
            }
            else if (dataSource is IDataSourceWithoutCaptcha dataSourceWithoutCaptcha)
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
                ReturnData(id, dataSourceResult, true);
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
        private void ReturnData(int id, DataSourceResult dataSourceResult, bool isSuccessfull)
        {
            var dataRequest = _requestsCache.Get(id);

            var result = new DataRequestResult()
            {
                Id = id,
                DataSourceResult = dataSourceResult,
                IsSuccessfull = isSuccessfull
            };

            dataRequest.CallBack(result);
        }
    }

   

    

    

}
