using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
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
                                      IVinCodeController vinCodeController,
                                      IFrameNumberController frameNumberController)
        {
            _logger = logger;
            _dataRequestController = dataRequestController;
            _licensePlateControllerCache = licensePlateControllerCache;
            _messagesSenderController = messagesSenderController;
            _queryExecutor = queryExecutor;
            _vinCodeController = vinCodeController;
            _frameNumberController = frameNumberController;
        }

        public async Task StartVinSearch(string licencePlate, int requestObjectId)
        {
            _logger.WriteToLog(LogLevel.Debug, $"Запущен поиск вин кода по гос. номеру {licencePlate}. {Environment.NewLine}" +
                                               $"Идентификатор объекта запроса: {requestObjectId}");

            await Start(licencePlate, requestObjectId);
        }

        private async Task Start(string licencePlate, int requestObjectId)
        {
            _licensePlateControllerCache.Add(requestObjectId, DataType.VechicleIdentifiersEAISTO, licencePlate);
            await _dataRequestController.StartDataSearch(requestObjectId, DataType.VechicleIdentifiersEAISTO, licencePlate, Callback);
        }

        private async Task EaistoDataHandler(int requestObjectId, VechicleIdentifiersData vid, bool isSuccssfull, string licencePlate)
        {
            if (isSuccssfull)
            {
                if (vid is null)
                {
                    _licensePlateControllerCache.Update(requestObjectId, DataType.VechicleIdentifiersRSA, true);
                    await _dataRequestController.StartDataSearch(requestObjectId, DataType.VechicleIdentifiersRSA, licencePlate, Callback);
                }
                else
                {
                    _licensePlateControllerCache.Remove(requestObjectId);
                    //var diagnosticCard = dcs.DiagnosticCards.OrderBy(x => x.DateFrom).First();

                    if (!string.IsNullOrWhiteSpace(vid.Vin))
                    {
                        await _queryExecutor.UpdateVinCode(requestObjectId, vid.Vin);
                        await _vinCodeController.StartGeneralInfoSearch(vid.Vin, requestObjectId);
                    }
                    else //if(!string.IsNullOrWhiteSpace(dc.FrameNumber))
                    {
                        //await _queryExecutor.UpdateFrameNumber(requestObjectId, vid.Vin);
                        await _frameNumberController.StartGeneralInfoSearch(vid.FrameNumber, requestObjectId);
                    }
                }
            }
            else
            {
                _licensePlateControllerCache.Update(requestObjectId, DataType.Osago, false);
                await _dataRequestController.StartDataSearch(requestObjectId, DataType.VechicleIdentifiersRSA, licencePlate, Callback);
            }
        }

        private async Task RsaDataHandler(int requestObjectId, VechicleIdentifiersData vid, bool isSuccssfull, string licensePlate, bool eaistoAvailable)
        {
            if (isSuccssfull)
            {
                if (vid is null)
                {
                    var requestObject = _queryExecutor.GetUserRequestObject(requestObjectId);
                    string error;

                    if (eaistoAvailable)
                        error = GetInfoNotFoundError(licensePlate);
                    else
                        error = StaticResources.RequestFailedError;

                    await _messagesSenderController.SendMessage(requestObject.UserId, error);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(vid.Vin))
                    {
                        await _queryExecutor.UpdateVinCode(requestObjectId, vid.Vin);
                        await _vinCodeController.StartGeneralInfoSearch(vid.Vin, requestObjectId);
                    }
                    else //if(!string.IsNullOrWhiteSpace(policy.FrameNumber))
                    {
                        //await _queryExecutor.UpdateFrameNumber(requestObjectId, vid.Vin);
                        await _frameNumberController.StartGeneralInfoSearch(vid.FrameNumber, requestObjectId);
                    }
                }
            }
            else
            {
                var requestObject = _queryExecutor.GetUserRequestObject(requestObjectId);
                await _messagesSenderController.SendMessage(requestObject.UserId, StaticResources.RequestFailedError);
            }
            _licensePlateControllerCache.Remove(requestObjectId);
        }

        private async Task Callback(DataRequestResult result)
        {
            var data = _licensePlateControllerCache.Get(result.Id);
            var vid = result.DataSourceResult?.Data as VechicleIdentifiersData;

            if (data.RequestedDataType == DataType.VechicleIdentifiersEAISTO)
            {
                await EaistoDataHandler(result.Id, vid, result.IsSuccessfull, data.LicensePlate);
            }
            else if (data.RequestedDataType == DataType.VechicleIdentifiersRSA)
            {
                await RsaDataHandler(result.Id, vid, result.IsSuccessfull, data.LicensePlate, data.DCSourcesNotAvailable);
            }
        }

        public string GetInfoNotFoundError(string licensePlate)
        {
            return $"😕 К сожалению не удалось найти информацию по гос. номеру {licensePlate}{Environment.NewLine}" +
                   $"Попробуйте выполнить поиск по VIN коду.";
        }
    }
}

