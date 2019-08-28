using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Models.RequestedDataCache;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using NLog;


namespace CheckAutoBot.Controllers
{
    public interface ILicensePlateController
    {
        Task StartVinSearch(string licencePlate, int requestObjectId);
    }

    public class LicensePlateController: ILicensePlateController
    {
        private ICustomLogger _logger;
        private IDataRequestController _dataRequestController;
        private ILicensePlateControllerCache _licensePlateControllerCache;
        private IMessagesSenderController _messagesSenderController;
        private DbQueryExecutor _queryExecutor;
        private IVinCodeController _vinCodeController;
        private IFrameNumberController _frameNumberController;


        public LicensePlateController(ICustomLogger logger,
                                      IDataRequestController dataRequestController,
                                      ILicensePlateControllerCache licensePlateControllerCache,
                                      IMessagesSenderController messagesSenderController,
                                      DbQueryExecutor queryExecutor,
                                      IVinCodeController vinCodeController)
        {
            _logger = logger;
            _dataRequestController = dataRequestController;
            _licensePlateControllerCache = licensePlateControllerCache;
            _messagesSenderController = messagesSenderController;
            _queryExecutor = queryExecutor;
            _vinCodeController = vinCodeController;
        }

        public async Task StartVinSearch(string licencePlate, int requestObjectId)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Запущен поиск вин кода по гос. номеру {licencePlate}. {Environment.NewLine}" +
                                               $"Идентификатор объекта запроса: {requestObjectId}");

            await Start(licencePlate, requestObjectId, DataType.DiagnosticCards);
        }

        private async Task Start(string licencePlate, int requestObjectId, DataType dataType)
        {
            _licensePlateControllerCache.Add(requestObjectId, DataType.DiagnosticCards, licencePlate);
            await _dataRequestController.StartDataSearch(requestObjectId, DataType.DiagnosticCards, licencePlate, Callback);
        }

        private async Task DiagnosticCardHandler(int requestObjectId, DiagnosticCard dc, bool isSuccssfull, string licencePlate)
        {
            if (isSuccssfull)
            {
                if (dc is null)
                {
                    _licensePlateControllerCache.Update(requestObjectId, DataType.Osago, true);
                    await _dataRequestController.StartDataSearch(requestObjectId, DataType.Osago, licencePlate, Callback);
                }
                else
                {
                    _licensePlateControllerCache.Remove(requestObjectId);

                    if (!string.IsNullOrWhiteSpace(dc.Vin))
                        await _vinCodeController.StartGeneralInfoSearch(dc.Vin, requestObjectId);
                    else //if(!string.IsNullOrWhiteSpace(dc.FrameNumber))
                        await _frameNumberController.StartGeneralInfoSearch(dc.FrameNumber, requestObjectId);
                }
            }
            else
            {
                _licensePlateControllerCache.Update(requestObjectId, DataType.Osago, false);
                await _dataRequestController.StartDataSearch(requestObjectId, DataType.Osago, licencePlate, Callback);
            }
        }

        private async Task PolicyOsagoHandler(int requestObjectId, PolicyOsago policy, bool isSuccssfull, string licensePlate, bool eaistoAvailable)
        {
            if (isSuccssfull)
            {
                if (policy is null)
                {
                    var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);
                    string error;

                    if (eaistoAvailable)
                        error = GetInfoNotFoundError(licensePlate);
                    else
                        error = StaticResources.RequestFailedError;

                    await _messagesSenderController.SendMessage(requestObject.UserId, error);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(policy.Vin))
                        await _vinCodeController.StartGeneralInfoSearch(policy.Vin, requestObjectId);
                    else //if(!string.IsNullOrWhiteSpace(policy.FrameNumber))
                        await _frameNumberController.StartGeneralInfoSearch(policy.FrameNumber, requestObjectId);
                }
            }
            else
            {
                var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);
                await _messagesSenderController.SendMessage(requestObject.UserId, StaticResources.RequestFailedError);
            }
            _licensePlateControllerCache.Remove(requestObjectId);
        }

        private async Task Callback(DataRequestResult result)
        {
            var data = _licensePlateControllerCache.Get(result.Id);

            if (data.RequestedDataType == DataType.DiagnosticCards)
            {
                var dc = result.DataSourceResult?.Data as DiagnosticCard;
                await DiagnosticCardHandler(result.Id, dc, result.IsSuccessfull, data.LicensePlate);
            }
            else if (data.RequestedDataType == DataType.Osago)
            {
                var policy = result.DataSourceResult?.Data as PolicyOsago;
                await PolicyOsagoHandler(result.Id, policy, result.IsSuccessfull, data.LicensePlate, data.DCSourcesNotAvailable);
            }
        }

        public string GetInfoNotFoundError(string licensePlate)
        {
            return $"😕 К сожалению не удалось найти информацию по гос. номеру {licensePlate}{Environment.NewLine}" +
                   $"Попробуйте выполнить поиск по VIN коду.";
        }
    }
}

