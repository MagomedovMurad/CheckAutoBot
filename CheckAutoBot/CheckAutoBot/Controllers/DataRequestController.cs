﻿using CheckAutoBot.DataSources.Contracts;
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
using CheckAutoBot.Managers;

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
        private RucaptchaManager _rucaptchaManager;

        public DataRequestController(IDataSource[] dataSources,
                                     IRequestedCaptchasCacheController captchasCacheController,
                                     IRequestedDataCacheController requestsCache,
                                     RucaptchaManager rucaptchaManager,
                                     ICustomLogger logger)
        {
            _dataSources = dataSources;
            _logger = logger;
            _requestsCache = requestsCache;
            _rucaptchaManager = rucaptchaManager;
            _captchasCacheController = captchasCacheController;
            _captchasCacheController.CaptchasSolved += SolvedCaptchasHandler;
        }

        public async Task StartDataSearch(int id, DataType dataType, object inputData, Func<DataRequestResult, Task> callBack)
        {
            try
            {
                var dataSource = _dataSources.Where(x => x.DataType.Equals(dataType)).Single(x => x.Order.Equals(1));
                _requestsCache.Add(id, dataSource, inputData, callBack);

                RequestData(id, inputData, dataSource);
            }
            catch (Exception ex)
            {
                _logger.WriteToLog(LogLevel.Error, $"Ошибка при запуске поиска данных: {ex}", true);
            }
        }
        private void SolvedCaptchasHandler(object sender, CaptchaRequestDataEnvelop envelop)
        {
            try
            {
                _logger.WriteToLog(LogLevel.Debug, $"Запуск обработки события решения каптчи (идентификатор запроса: {envelop.Id})");
                var dataRequest = _requestsCache.Get(envelop.Id);
                GetDataFromSource(envelop.Id, dataRequest.DataSource, dataRequest.InputData, envelop.CaptchaRequestDataList);
            }
            catch (Exception ex)
            {
                _logger.WriteToLog(LogLevel.Error, $"Ошибка обработки события решения каптчи (идентификатор запроса: {envelop.Id}):{ex}");
            }
        }

        private void RepeatRequest(int id)
        {
            var dataRequest = _requestsCache.Get(id);
            if (dataRequest.RepeatCount >= dataRequest.DataSource.MaxRepeatCount)
            {
                var dataSource = _dataSources.Where(x => x.DataType.Equals(dataRequest.DataSource.DataType))
                                             .FirstOrDefault(x => x.Order.Equals(dataRequest.DataSource.Order + 1));

                if (dataSource is null)
                {
                    _logger.WriteToLog(LogLevel.Error, $"Все источники данных опрошены. Данные не найдены.");
                    ReturnData(id, null, null, null, false);
                }
                else
                {
                    _logger.WriteToLog(LogLevel.Error, $"Повторный запрос ({dataRequest.RepeatCount + 1}-й) данных из источников. Идентификатор ({id})");
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

        private void RequestCaptcha(int id, IDataSourceWithCaptcha dataSource)
        {
            try
            {
                _logger.WriteToLog(LogLevel.Debug, $"Запрос каптч для выполнения запроса. " +
                                                   $"Идентификатор запроса: {id}." +
                                                   $"Тип: {dataSource.DataType}. " +
                                                   $"Источник данных: {dataSource.Name}");
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

        private void GetDataFromSource(int id, IDataSource dataSource, object inputData, IEnumerable<CaptchaRequestData> captchas)
        {
            try
            {
                CheckCaptchas(captchas);
                _logger.WriteToLog(LogLevel.Debug, $"Запрос данных из источника. " +
                                                   $"Идентификатор запроса: {id}." +
                                                   $"Тип: {dataSource.DataType}. " +
                                                   $"Источник данных: {dataSource.Name}");
                var dataSourceResult = dataSource.GetData(inputData, captchas);
                ReturnData(id, dataSourceResult, dataSource.DataType, dataSource.Name, true);

                if (captchas != null)
                    SendCaptchaReports(captchas, true);
                return;
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidCaptchaException icEx)
                {
                    var warn = $"Неверно решена каптча. {Environment.NewLine}" +
                               $"Ответ: {icEx.CaptchaWord}. " +
                               $"Тип данных: { dataSource.DataType}";
                    _logger.WriteToLog(LogLevel.Warn, warn, true);

                    var crd = captchas.Single(x => x.Value.Equals(icEx.CaptchaWord));
                    SendCaptchaReports(new[] { crd }, false);
                }
                else
                {
                    var error = $"Идентификатор запроса: {id}.{Environment.NewLine}" +
                                $"Тип данных: {dataSource.DataType}." +
                                $"Ошибка: {ex.Message}. {ex.InnerException?.Message}";
                    _logger.WriteToLog(LogLevel.Error, error, true);
                }
            }
            catch (Exception ex)
            {
                var error = $"Непредвиденная ошибка при запросе данных. {Environment.NewLine}" +
                            $"Идентификатор запроса: {id}.{Environment.NewLine}" +
                            $"Тип данных: {dataSource.DataType}.{Environment.NewLine}" +
                            $"Ошибка: {ex.Message}. {ex}";
                _logger.WriteToLog(LogLevel.Error, error, true);
            }
            RepeatRequest(id);
        }

        private void SendCaptchaReports(IEnumerable<CaptchaRequestData> captchas, bool isGood)
        {
            foreach (var captcha in captchas)
            {
                _logger.WriteToLog(isGood? LogLevel.Debug : LogLevel.Warn, $"Отправка отчета по решенной каптче с идентификатором {captcha.CaptchaId}: " +
                                                   $"решена {(isGood ? "ВЕРНО" : "НЕВЕРНО")}");
                _rucaptchaManager.SendReport(captcha.CaptchaId, false);
            }
        }

        private void CheckCaptchas(IEnumerable<CaptchaRequestData> captchas)
        {
            if (captchas is null)
                return;

            foreach (var captcha in captchas)
                RucaptchaManager.CheckCaptchaWord(captcha.Value);
        }

        private void ReturnData(int id, DataSourceResult dataSourceResult, DataType? dataType, string dataSourceName, bool isSuccessfull)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Возврат найденных данных с идентификатором {id}");

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
